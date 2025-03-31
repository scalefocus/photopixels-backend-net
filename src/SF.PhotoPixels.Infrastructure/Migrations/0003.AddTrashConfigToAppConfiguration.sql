DO $$ 
DECLARE 
    my_uuid UUID;
BEGIN
    Select id
    INTO my_uuid
    FROM photos.mt_doc_applicationconfiguration LIMIT 1;
UPDATE photos.mt_doc_applicationConfiguration
	SET data = jsonb_set(data, '{TrashHardDeleteConfiguration}', '{}', true)
	WHERE id = my_uuid;

UPDATE photos.mt_doc_applicationConfiguration
	SET data = jsonb_set(data, '{TrashHardDeleteConfiguration, LastRun}', '"0001-01-01T00:00:00"', true)
	WHERE id = my_uuid;

UPDATE photos.mt_doc_applicationConfiguration
	SET data = jsonb_set(data, '{TrashHardDeleteConfiguration, DeleteAtTimeOfDay}', '"00:00:00"', true)
	WHERE id = my_uuid;

UPDATE photos.mt_doc_applicationConfiguration
	SET data = jsonb_set(data, '{TrashHardDeleteConfiguration, DaysToDelayHardDelete}', to_jsonb(30), true)
	WHERE id = my_uuid;
END $$;