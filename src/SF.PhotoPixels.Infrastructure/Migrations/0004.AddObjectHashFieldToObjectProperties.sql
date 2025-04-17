DO $$ 
DECLARE 
    my_uuid UUID;
BEGIN
    
UPDATE photos.mt_doc_applicationConfiguration
	SET data = jsonb_set(data, '{OriginalHash}', '', true)
END $$;