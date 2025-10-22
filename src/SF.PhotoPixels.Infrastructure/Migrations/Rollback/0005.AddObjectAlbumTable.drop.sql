DROP TABLE IF EXISTS photos.mt_doc_albumobject CASCADE;
drop function if exists photos.mt_upsert_albumobject(JSONB, varchar, varchar, uuid);
drop function if exists photos.mt_insert_albumobject(JSONB, varchar, varchar, uuid);
drop function if exists photos.mt_update_albumobject(JSONB, varchar, varchar, uuid);
