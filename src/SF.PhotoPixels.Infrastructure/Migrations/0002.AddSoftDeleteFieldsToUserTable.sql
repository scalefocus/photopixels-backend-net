ALTER TABLE mt_doc_user
ADD COLUMN IF NOT EXISTS mt_deleted BOOLEAN DEFAULT FALSE, 
ADD COLUMN IF NOT EXISTS mt_deleted_at TIMESTAMP;
