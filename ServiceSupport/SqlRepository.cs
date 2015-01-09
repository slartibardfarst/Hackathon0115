using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using SDS.Providers.MPRRouter;
using ServiceSupport;

namespace ImageHashingTest
{
    public class SqlRepository
    {
        private MPRRedirect _mprRedirect;

        public SqlRepository(MPRRedirect dataSourceMprRedirect)
        {
            _mprRedirect = dataSourceMprRedirect;
        }

        public IEnumerable<ListingImageDetails> QueryListingImages(int partitionId, int max, string queryPrefix, string whereClause)
        {
            queryPrefix = string.IsNullOrEmpty(queryPrefix) ? "" : queryPrefix;
            whereClause = string.IsNullOrEmpty(whereClause) ? "" : whereClause;

            string sqlTemplate = @"
                            SELECT {0}
	                            pei.mpr_id as MprId,
	                            ml.master_listing_id as MlId, 
	                            lm.listing_id as ListingId, 
	                            lm.file_url as ImageUrl,
	                            lda.address_line AddressLine,
	                            lda.street_number as StreetNumber,
	                            lda.street_direction as StreetDirection,
	                            lda.street_name as StreetName,
	                            lda.street_suffix as StreetSuffix,
	                            lda.street_post_dir as StreetPostDirection,
	                            lda.unit_value as UnitValue,
	                            lda.city as City, 
	                            lda.state_code as State, 
	                            lda.zip as Zip
                            FROM Property.dataagg.listings l WITH(NOLOCK)
                            INNER JOIN [Property].[dataagg].[listing_media] lm WITH(NOLOCK) ON l.listing_id = lm.listing_id
                            join masterpropertyrecord.dbo.property_external_ids pei (NOLOCK) on l.state_code + CONVERT(varchar,l.property_id) = pei.property_id
                            join property.dataagg.master_listings ml (NOLOCK) on ml.listing_id = l.listing_id 
                            left outer join property.dataagg.listing_display_address lda (NOLOCK) on lda.listing_id = l.listing_id and lda.state_code = l.state_code
                            WHERE l.listing_id IN (
	                            SELECT TOP {1} l.listing_id 
	                            FROM Property.dataagg.listings l WITH(NOLOCK)
                                {2} )";

            string sql = string.Format(sqlTemplate, queryPrefix, max, whereClause);
            string connectionString = _mprRedirect.GetConnectionStringByPartitionId(partitionId, "property");
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var items = dbConnection.Query<ListingImageDetails>(sql, commandTimeout: 9800);
                foreach (var item in items)
                    yield return item;
            }
        }

        public void UpsertListingImageDetails(int partitionId, string tableName, ListingImageDetails image)
        {
            const string updateSqlTemplate = @"BEGIN TRAN
                                               UPDATE   {0}
                                               SET 
                                                        [mpr_id]	        = @mpr_id,
                                                        [master_listing_id] = @master_listing_id,
	                                                    [listing_id]	    = @listing_id,
	                                                    [image_url]		    = @image_url,
	                                                    [image_hash]	    = @image_hash,
	                                                    [address_line]	    = @address_line,
                                                        [street_number]     = @street_number,
                                                        [street_direction]  = @street_direction,
                                                        [street_name]       = @street_name,
                                                        [street_suffix]     = @street_suffix,
                                                        [street_post_direction]   = @street_post_direction,
                                                        [unit_value]        = @unit_value,
	                                                    [city]              = @city,
	                                                    [state]		        = @state,
	                                                    [zip]			    = @zip
                                               WHERE listing_id = @listing_id AND
                                                     state      = @state AND
                                                     image_url  = @image_url
                                               IF @@ROWCOUNT = 0
                                               BEGIN
                                                  INSERT INTO {0}
                                                  (
                                                        [mpr_id]	       ,
                                                        [master_listing_id],
	                                                    [listing_id]	   ,
	                                                    [image_url]		   ,
	                                                    [image_hash]	   ,
	                                                    [address_line]	   ,
                                                        [street_number]    ,
                                                        [street_direction] ,
                                                        [street_name]      ,
                                                        [street_suffix]    ,
                                                        [street_post_direction]  ,
                                                        [unit_value]       ,
	                                                    [city]             ,
	                                                    [state]		       ,
	                                                    [zip]			   
                                                  )
                                                  VALUES
                                                  (
                                                        @mpr_id,
                                                        @master_listing_id,
                                                        @listing_id,
	                                                    @image_url,
	                                                    @image_hash,
	                                                    @address_line,
                                                        @street_number,
                                                        @street_direction,
                                                        @street_name,
                                                        @street_suffix,
                                                        @street_post_direction,
                                                        @unit_value,
	                                                    @city,
	                                                    @state,
	                                                    @zip
                                                   )
                                               END
                                            COMMIT TRAN";

            try
            {
                string connectionString = _mprRedirect.GetConnectionStringByPartitionId(partitionId, "MasterPropertyRecord");
                using (var dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();
                    string sql = string.Format(updateSqlTemplate, tableName);
                    SqlCommand cmd = new SqlCommand(sql, dbConnection);

                    cmd.Parameters.AddWithValue("mpr_id", image.MprId);
                    cmd.Parameters.AddWithValue("master_listing_id", image.MlId);
                    cmd.Parameters.AddWithValue("listing_id", image.ListingId);
                    cmd.Parameters.AddWithValue("image_url", ((object)image.ImageUrl) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("image_hash", (decimal)image.ImageHash);
                    cmd.Parameters.AddWithValue("address_line", ((object)image.AddressLine) ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("street_number", ((object)image.StreetNumber) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("street_direction", ((object)image.StreetDirection) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("street_name", ((object)image.StreetName) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("street_suffix", ((object)image.StreetSuffix) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("street_post_direction", ((object)image.StreetPostDirection) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("unit_value", ((object)image.UnitValue) ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("city", ((object)image.City) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("state", ((object)image.State) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("zip", ((object)image.Zip) ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<PhotoIssue> QueryForSharedImagesHashes(string stateCode, int zip, int minDuplicates, string whereClause)
        {
            var result = new List<PhotoIssue>();

            var sqlTemplate = @"select 
                            image_hash as ImageHash, 
                            count(*) as NumListingsSharingImage
                        from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2] 
                        where image_hash in 
                        (
	                        select a1.image_hash
	                        from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2] a1
	                        join [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2] a2 on a1.image_hash = a2.image_hash
	                        where a1.mpr_id <> a2.mpr_id 
	                              and a1.zip = @zipcode
                                  {0}
                        ) 
                        group by image_hash 
                        having count(*) > 2
                        order by count(*) desc";

            var sql = string.Format(sqlTemplate, string.IsNullOrEmpty(whereClause) ? "" : "and " + whereClause);
            string connectionString = _mprRedirect.GetConnectionStringByStateCode(stateCode, "MasterPropertyRecord");
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var items = dbConnection.Query<PhotoIssue>(sql, new { zipcode = zip }, commandTimeout: 9800);
                foreach (var item in items)
                {
                    item.StateCode = stateCode;
                    item.Zip = zip;
                    result.Add(item);
                }
            }

            return result;
        }


        public List<ListingImage> GetListingsSharingImageHash(string stateCode, ulong sharedHash)
        {
            var result = new List<ListingImage>();

            var sql = @"select 
                            mpr_id as MprId,
                            master_listing_id as MlId,
                            listing_id as ListingId,
                            image_url as ImageUrl,
                            image_hash as ImageHash,
                            address_line as AddressLine,
                            city as City,
                            state as State,
                            zip as Zip
                        from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2]
                        where image_hash = @imageHash";

            string connectionString = _mprRedirect.GetConnectionStringByStateCode(stateCode, "MasterPropertyRecord");
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var items = dbConnection.Query<ListingImage>(sql, new { imageHash = (decimal)sharedHash }, commandTimeout: 9800);
                foreach (var item in items)
                    result.Add(item);
            }

            return result;
        }





        public List<string> GetAllImagesForListing(int listingId, string stateCode)
        {
            var result = new List<string>();

            var sql = @"select image_url
                        from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2]
                        where listing_id = @listingID
                              and state = @state";

            string connectionString = _mprRedirect.GetConnectionStringByStateCode(stateCode, "MasterPropertyRecord");
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var items = dbConnection.Query<string>(sql, new { listingID = listingId, state = stateCode }, commandTimeout: 9800);
                foreach (var item in items)
                    result.Add(item);
            }

            return result;
        }

        public string QueryForFirstUrlGivenHash(string stateCode, ulong imageHash)
        {
            var sql = @"select top 1 image_url
                        from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2]
                        where image_hash = @image_hash";

            string connectionString = _mprRedirect.GetConnectionStringByStateCode(stateCode, "MasterPropertyRecord");
            using (var dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query<string>(sql, new {image_hash = (decimal) imageHash}, commandTimeout: 9800).FirstOrDefault();
                return result;
            }

            return null;
        }
    }
}
