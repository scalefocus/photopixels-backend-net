
DO $$ 
DECLARE 
    my_uuid UUID;
BEGIN
-- Remove the nested properties in reverse order
UPDATE photos.mt_doc_objectproperties
    SET data = data - '{OriginalHash}';
END $$;