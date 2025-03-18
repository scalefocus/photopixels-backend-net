ALTER TABLE mt_doc_user 
DROP COLUMN IF EXISTS mt_deleted, 
DROP COLUMN IF EXISTS mt_deleted_at;
