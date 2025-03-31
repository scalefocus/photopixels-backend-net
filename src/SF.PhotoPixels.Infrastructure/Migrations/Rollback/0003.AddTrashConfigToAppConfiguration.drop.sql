
DO $$ 
DECLARE 
    my_uuid UUID;
BEGIN
    -- Retrieve the ID of the application configuration
    SELECT id
    INTO my_uuid
    FROM photos.mt_doc_applicationconfiguration
    LIMIT 1;

-- Remove the nested properties in reverse order
UPDATE photos.mt_doc_applicationconfiguration
    SET data = data - '{TrashHardDeleteConfiguration,DaysToDelayHardDelete}'
    WHERE id = my_uuid;

UPDATE photos.mt_doc_applicationconfiguration
    SET data = data - '{TrashHardDeleteConfiguration,DeleteAtTimeOfDay}'
    WHERE id = my_uuid;

UPDATE photos.mt_doc_applicationconfiguration
    SET data = data - '{TrashHardDeleteConfiguration,LastRun}'
    WHERE id = my_uuid;

-- Finally, remove the parent property
UPDATE photos.mt_doc_applicationconfiguration
    SET data = data - 'TrashHardDeleteConfiguration'
    WHERE id = my_uuid;
END $$;