CREATE SCHEMA IF NOT EXISTS photos;

CREATE
OR REPLACE FUNCTION photos.mt_immutable_timestamp(value text) RETURNS timestamp without time zone LANGUAGE sql IMMUTABLE AS
$function$
select value::timestamp

$function$;


CREATE
OR REPLACE FUNCTION photos.mt_immutable_timestamptz(value text) RETURNS timestamp with time zone LANGUAGE sql IMMUTABLE AS
$function$
select value::timestamptz

$function$;


CREATE
OR REPLACE FUNCTION photos.mt_grams_vector(text)
        RETURNS tsvector
        LANGUAGE plpgsql
        IMMUTABLE STRICT
AS $function$
BEGIN
RETURN (SELECT array_to_string(photos.mt_grams_array($1), ' ') ::tsvector);
END
$function$;


CREATE
OR REPLACE FUNCTION photos.mt_grams_query(text)
        RETURNS tsquery
        LANGUAGE plpgsql
        IMMUTABLE STRICT
AS $function$
BEGIN
RETURN (SELECT array_to_string(photos.mt_grams_array($1), ' & ') ::tsquery);
END
$function$;


CREATE
OR REPLACE FUNCTION photos.mt_grams_array(words text)
        RETURNS text[]
        LANGUAGE plpgsql
        IMMUTABLE STRICT
AS $function$
        DECLARE
result text[];
        DECLARE
word text;
        DECLARE
clean_word text;
BEGIN
                FOREACH
word IN ARRAY string_to_array(words, ' ')
                LOOP
                     clean_word = regexp_replace(word, '[^a-zA-Z0-9]+', '','g');
FOR i IN 1 .. length(clean_word)
                     LOOP
                         result := result || quote_literal(substr(lower(clean_word), i, 1));
                         result
:= result || quote_literal(substr(lower(clean_word), i, 2));
                         result
:= result || quote_literal(substr(lower(clean_word), i, 3));
END LOOP;
END LOOP;

RETURN ARRAY(SELECT DISTINCT e FROM unnest(result) AS a(e) ORDER BY e);
END;
$function$;


DROP TABLE IF EXISTS photos.mt_doc_applicationconfiguration CASCADE;
CREATE TABLE photos.mt_doc_applicationconfiguration (
    id                  uuid                        NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
CONSTRAINT pkey_mt_doc_applicationconfiguration_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_applicationconfiguration_idx_id ON photos.mt_doc_applicationconfiguration USING btree ((id));

CREATE OR REPLACE FUNCTION photos.mt_upsert_applicationconfiguration(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_applicationconfiguration ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_applicationconfiguration into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_insert_applicationconfiguration(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_applicationconfiguration ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_applicationconfiguration(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_applicationconfiguration SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_applicationconfiguration into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP TABLE IF EXISTS photos.mt_doc_deadletterevent CASCADE;
CREATE TABLE photos.mt_doc_deadletterevent (
    id                  uuid                        NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
CONSTRAINT pkey_mt_doc_deadletterevent_id PRIMARY KEY (id)
);

CREATE OR REPLACE FUNCTION photos.mt_upsert_deadletterevent(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_deadletterevent ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_deadletterevent into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_insert_deadletterevent(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_deadletterevent ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_deadletterevent(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_deadletterevent SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_deadletterevent into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP TABLE IF EXISTS photos.mt_doc_objectproperties CASCADE;
CREATE TABLE photos.mt_doc_objectproperties (
    id                  varchar                     NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    user_id             uuid                        NULL,
    mt_deleted          boolean                     NULL DEFAULT FALSE,
    mt_deleted_at       timestamp with time zone    NULL,
CONSTRAINT pkey_mt_doc_objectproperties_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_objectproperties_idx_mt_deleted ON photos.mt_doc_objectproperties USING btree (mt_deleted);

CREATE INDEX mt_doc_objectproperties_idx_mt_deleted_at ON photos.mt_doc_objectproperties USING btree (mt_deleted_at) WHERE (mt_deleted);

CREATE INDEX mt_doc_objectproperties_idx_hash ON photos.mt_doc_objectproperties USING btree ((data ->> 'Hash'));

CREATE INDEX mt_doc_objectproperties_idx_user_id ON photos.mt_doc_objectproperties USING btree (user_id);

CREATE OR REPLACE FUNCTION photos.mt_upsert_objectproperties(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
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


CREATE OR REPLACE FUNCTION photos.mt_insert_objectproperties(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_objectproperties ("user_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_user_id, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_objectproperties(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_objectproperties SET "user_id" = arg_user_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_objectproperties into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP TABLE IF EXISTS photos.mt_doc_user CASCADE;
CREATE TABLE photos.mt_doc_user (
    id                  uuid                        NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
CONSTRAINT pkey_mt_doc_user_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_user_idx_id ON photos.mt_doc_user USING btree ((id));

CREATE OR REPLACE FUNCTION photos.mt_upsert_user(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
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


CREATE OR REPLACE FUNCTION photos.mt_insert_user(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_user ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_user(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_user SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_user into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

DROP TABLE IF EXISTS photos.mt_streams CASCADE;
CREATE TABLE photos.mt_streams (
    id                  uuid           NOT NULL,
    type                varchar        NULL,
    version             bigint         NULL,
    timestamp           timestamptz    NOT NULL DEFAULT (now()),
    snapshot            jsonb          NULL,
    snapshot_version    integer        NULL,
    created             timestamptz    NOT NULL DEFAULT (now()),
    tenant_id           varchar        NULL DEFAULT '*DEFAULT*',
    is_archived         bool           NULL DEFAULT FALSE,
CONSTRAINT pkey_mt_streams_id PRIMARY KEY (id)
);
DROP TABLE IF EXISTS photos.mt_events CASCADE;
CREATE TABLE photos.mt_events (
    seq_id            bigint                      NOT NULL,
    id                uuid                        NOT NULL,
    stream_id         uuid                        NULL,
    version           bigint                      NOT NULL,
    data              jsonb                       NOT NULL,
    type              varchar(500)                NOT NULL,
    timestamp         timestamp with time zone    NOT NULL DEFAULT '(now())',
    tenant_id         varchar                     NULL DEFAULT '*DEFAULT*',
    mt_dotnet_type    varchar                     NULL,
    is_archived       bool                        NULL DEFAULT FALSE,
CONSTRAINT pkey_mt_events_seq_id PRIMARY KEY (seq_id)
);

ALTER TABLE photos.mt_events
ADD CONSTRAINT fkey_mt_events_stream_id FOREIGN KEY(stream_id)
REFERENCES photos.mt_streams(id)ON DELETE CASCADE
;


CREATE UNIQUE INDEX pk_mt_events_stream_and_version ON photos.mt_events USING btree (stream_id, version);

CREATE UNIQUE INDEX pk_mt_events_id_unique ON photos.mt_events USING btree (id);
CREATE SEQUENCE photos.mt_events_sequence;
ALTER SEQUENCE photos.mt_events_sequence OWNED BY photos.mt_events.seq_id;
DROP TABLE IF EXISTS photos.mt_event_progression CASCADE;
CREATE TABLE photos.mt_event_progression (
    name            varchar                     NOT NULL,
    last_seq_id     bigint                      NULL,
    last_updated    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
CONSTRAINT pk_mt_event_progression PRIMARY KEY (name)
);
CREATE
OR REPLACE FUNCTION photos.mt_mark_event_progression(name varchar, last_encountered bigint) RETURNS VOID LANGUAGE plpgsql AS
$function$
BEGIN
INSERT INTO photos.mt_event_progression (name, last_seq_id, last_updated)
VALUES (name, last_encountered, transaction_timestamp())
ON CONFLICT ON CONSTRAINT pk_mt_event_progression
    DO
UPDATE SET last_seq_id = last_encountered, last_updated = transaction_timestamp();

END;

$function$;



CREATE OR REPLACE FUNCTION photos.mt_archive_stream(streamid uuid) RETURNS VOID LANGUAGE plpgsql AS
$function$
BEGIN
  update photos.mt_streams set is_archived = TRUE where id = streamid;
  update photos.mt_events set is_archived = TRUE where stream_id = streamid;
END;
$function$;

