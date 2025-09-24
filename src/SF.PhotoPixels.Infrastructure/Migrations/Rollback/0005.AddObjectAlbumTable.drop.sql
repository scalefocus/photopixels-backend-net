DROP TABLE IF EXISTS photos.mt_doc_albumobject CASCADE;
drop function if exists photos.mt_upsert_objectalbum(JSONB, varchar, varchar, uuid);
drop function if exists photos.mt_insert_objectalbum(JSONB, varchar, varchar, uuid);
drop function if exists photos.mt_update_objectalbum(JSONB, varchar, varchar, uuid);
