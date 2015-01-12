select * from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]
order by image_hash desc


select count(*) from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]
where zip = 11215

select count(*) from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2]
where zip = 11215

select zip, count(*) as cnt
from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]
group by zip
order by zip desc




select image_hash, count(*) as cnt 
from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]
group by image_hash
having count(*) > 1
order by cnt desc


select top 10 * from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]


select top 10 
image_hash, count(*) as cnt 
from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]
group by image_hash
having count(*) = 2 
order by cnt desc


select top 10 *
from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]
where image_hash = 7057688112513802689


select *
from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2] 
where image_hash in 
(
	select a1.image_hash
	from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2] a1
	join [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2] a2 on a1.image_hash = a2.image_hash
	where a1.mpr_id <> a2.mpr_id
) 
order by image_hash desc




select top 1 *
from [MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes_try2]
where zip = 11215




SELECT 
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
	SELECT TOP 10 l.listing_id 
	FROM Property.dataagg.listings l WITH(NOLOCK)
    WHERE l.state_code = 'NY' AND l.listing_status_id = 6 )


SELECT count(*)
FROM Property.dataagg.listings l WITH(NOLOCK)
INNER JOIN [Property].[dataagg].[listing_media] lm WITH(NOLOCK) ON l.listing_id = lm.listing_id
join masterpropertyrecord.dbo.property_external_ids pei (NOLOCK) on l.state_code + CONVERT(varchar,l.property_id) = pei.property_id
join property.dataagg.master_listings ml (NOLOCK) on ml.listing_id = l.listing_id 
left outer join property.dataagg.listing_display_address lda (NOLOCK) on lda.listing_id = l.listing_id and lda.state_code = l.state_code
WHERE l.listing_id IN (
	SELECT TOP 100000 l.listing_id 
	FROM Property.dataagg.listings l WITH(NOLOCK)
    WHERE l.state_code = 'NY' AND l.listing_status_id = 6 )