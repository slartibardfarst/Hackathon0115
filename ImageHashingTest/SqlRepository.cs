using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using SDS.Providers.MPRRouter;

namespace ImageHashingTest
{
    public class SqlRepository
    {
        private MPRRedirect _mprRedirect;

        public SqlRepository(MPRRedirect dataSourceMprRedirect)
        {
            _mprRedirect = dataSourceMprRedirect;
        }

        internal IEnumerable<ListingImageDetails> QueryListingImages(int partitionId, int max, string queryPrefix, string whereClause)
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

        internal void UpsertListingImageDetails(int partitionId, string tableName, ListingImageDetails image)
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
	                                                    [city]              = @city,
	                                                    [state]		        = @state,
	                                                    [zip]			    = @zip
                                               WHERE listing_id         = @listing_id AND
                                                     state              = @state AND
                                                     image_url          = @image_url
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
                    cmd.Parameters.AddWithValue("image_hash", (decimal) image.ImageHash);
                    cmd.Parameters.AddWithValue("address_line", ((object)image.AddressLine) ?? DBNull.Value);
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
    }
}
