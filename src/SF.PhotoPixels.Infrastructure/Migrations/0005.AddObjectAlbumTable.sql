DROP TABLE IF EXISTS photos.mt_doc_objectalbum CASCADE;
CREATE TABLE photos.mt_doc_objectalbum (
    id               varchar                     NOT NULL,
    data             jsonb                       NOT NULL,
    mt_last_modified timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version       uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type   varchar                     NULL,    
    object_id        varchar                     NULL,
    album_id         varchar                     NULL,    
    CONSTRAINT pkey_mt_doc_objectalbum_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_objectalbum_idx_id ON photos.mt_doc_objectalbum USING btree ((id));

CREATE OR REPLACE FUNCTION photos.mt_upsert_objectalbum(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_objectalbum ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_objectalbum into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_insert_objectalbum(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_objectalbum ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_objectalbum(doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_objectalbum SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_objectalbum into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

