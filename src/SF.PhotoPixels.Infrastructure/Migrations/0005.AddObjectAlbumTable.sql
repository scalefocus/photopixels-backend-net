DROP TABLE IF EXISTS photos.mt_doc_albumobject CASCADE;
CREATE TABLE photos.mt_doc_albumobject (
    id               varchar                     NOT NULL,
    data             jsonb                       NOT NULL,
    album_id         varchar                     NULL,
    object_id        varchar                     NULL,
    mt_last_modified timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version       uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type   varchar                     NULL,        
    CONSTRAINT pkey_mt_doc_albumobject_id PRIMARY KEY (id)
);

-- add foreign key constraints and unique constraints
ALTER TABLE photos.mt_doc_albumobject
  ADD COLUMN IF NOT EXISTS album_id varchar,
  ADD COLUMN IF NOT EXISTS object_id varchar;

UPDATE photos.mt_doc_albumobject
SET album_id = COALESCE(album_id, data->>'AlbumId'),
    object_id = COALESCE(object_id, data->>'ObjectId')
WHERE (album_id IS NULL OR object_id IS NULL);

ALTER TABLE photos.mt_doc_albumobject
  ALTER COLUMN album_id SET NOT NULL,
  ALTER COLUMN object_id SET NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_albumobject_album_object
  ON photos.mt_doc_albumobject (album_id, object_id);

ALTER TABLE photos.mt_doc_albumobject
  DROP CONSTRAINT IF EXISTS mt_doc_albumobject_album_id_fkey;

ALTER TABLE photos.mt_doc_albumobject
  ADD CONSTRAINT mt_doc_albumobject_album_id_fkey
  FOREIGN KEY (album_id) REFERENCES photos.mt_doc_album(id)
  ON DELETE RESTRICT;

CREATE OR REPLACE FUNCTION photos.ensure_object_exists_not_deleted() RETURNS trigger AS $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM photos.mt_doc_objectproperties op
    WHERE op.id = NEW.object_id
      AND op.mt_deleted = false
  ) THEN
    RAISE EXCEPTION 'Object % does not exist or is soft-deleted', NEW.object_id
      USING ERRCODE = '23503';
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_albumobject_check_object ON photos.mt_doc_albumobject;

CREATE TRIGGER trg_albumobject_check_object
BEFORE INSERT OR UPDATE OF object_id
ON photos.mt_doc_albumobject
FOR EACH ROW
EXECUTE FUNCTION photos.ensure_object_exists_not_deleted();

-- end of constraint logic (the whole block can be emmited if not needed anymore)

CREATE INDEX mt_doc_albumobject_idx_id ON photos.mt_doc_albumobject USING btree ((id));

CREATE OR REPLACE FUNCTION photos.mt_upsert_albumobject(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_albumobject ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_albumobject into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_insert_albumobject(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_albumobject ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_albumobject(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_albumobject SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_albumobject into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

