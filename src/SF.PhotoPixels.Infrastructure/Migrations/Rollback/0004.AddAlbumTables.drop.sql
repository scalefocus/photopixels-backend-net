drop function if exists photos.mt_immutable_time(text);
drop function if exists photos.mt_immutable_date(text);
drop function if exists photos.mt_jsonb_append(, , , , default null::jsonb);
drop function if exists photos.mt_jsonb_copy(, , );
drop function if exists photos.mt_jsonb_duplicate(, , );
drop function if exists photos.mt_jsonb_fix_null_parent(, );
drop function if exists photos.mt_jsonb_increment(, , );
drop function if exists photos.mt_jsonb_insert(, , , , , default null::jsonb);
drop function if exists photos.mt_jsonb_move(, , );
drop function if exists photos.mt_jsonb_path_to_array(, );
drop function if exists photos.mt_jsonb_remove(, , );
drop function if exists photos.mt_jsonb_patch(, );
DROP TABLE IF EXISTS photos.mt_doc_album CASCADE;
drop function if exists photos.mt_upsert_album(uuid, JSONB, varchar, varchar, uuid);
drop function if exists photos.mt_insert_album(uuid, JSONB, varchar, varchar, uuid);
drop function if exists photos.mt_update_album(uuid, JSONB, varchar, varchar, uuid);
alter table photos.mt_doc_objectproperties drop constraint if exists pkey_mt_doc_objectproperties_id_mt_deleted;
alter table photos.mt_doc_objectproperties add CONSTRAINT pkey_mt_doc_objectproperties_id PRIMARY KEY (id);
drop function if exists photos.mt_upsert_objectproperties(uuid, JSONB, varchar, varchar, uuid) cascade;
CREATE OR REPLACE FUNCTION photos.mt_upsert_objectproperties(arg_user_id uuid, doc jsonb, docdotnettype character varying, docid character varying, docversion uuid)
 RETURNS uuid
 LANGUAGE plpgsql
AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_objectproperties ("user_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_user_id, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "user_id" = arg_user_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_objectproperties into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;
alter table photos.mt_doc_user alter column mt_deleted_at type timestamp without time zone;
drop index if exists photos.mt_doc_user_idx_mt_deleted;
drop index if exists photos.mt_doc_user_idx_mt_deleted_at;
alter table photos.mt_doc_user drop constraint if exists pkey_mt_doc_user_id_mt_deleted;
alter table photos.mt_doc_user add CONSTRAINT pkey_mt_doc_user_id PRIMARY KEY (id);
drop function if exists photos.mt_upsert_user(JSONB, varchar, uuid, uuid) cascade;
CREATE OR REPLACE FUNCTION photos.mt_upsert_user(doc jsonb, docdotnettype character varying, docid uuid, docversion uuid)
 RETURNS uuid
 LANGUAGE plpgsql
AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_user ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_user into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;
CREATE UNIQUE INDEX pk_mt_events_id_unique ON photos.mt_events USING btree (id);
drop function if exists photos.mt_quick_append_events(uuid, varchar, varchar, uuid[], varchar[], varchar[], jsonb[]);
