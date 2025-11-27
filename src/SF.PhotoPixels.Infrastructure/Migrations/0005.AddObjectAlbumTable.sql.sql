DROP FUNCTION IF EXISTS photos.mt_upsert_albumobject(arg_album_id character varying, arg_object_id character varying, doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION photos.mt_upsert_albumobject(arg_album_id varchar, arg_object_id varchar, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_albumobject ("album_id", "object_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_album_id, arg_object_id, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "album_id" = arg_album_id, "object_id" = arg_object_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_albumobject into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

