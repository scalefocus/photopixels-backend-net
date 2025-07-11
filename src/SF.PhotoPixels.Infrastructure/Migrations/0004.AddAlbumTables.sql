CREATE
OR REPLACE FUNCTION photos.mt_immutable_time(value text) RETURNS time without time zone LANGUAGE sql IMMUTABLE AS
$function$
select value::time

$function$;


CREATE
OR REPLACE FUNCTION photos.mt_immutable_date(value text) RETURNS date LANGUAGE sql IMMUTABLE AS
$function$
select value::date

$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_append(jsonb, text[], jsonb, boolean, jsonb default null::jsonb)
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    location ALIAS FOR $2;
    val ALIAS FOR $3;
    if_not_exists ALIAS FOR $4;
    patch_expression ALIAS FOR $5;
    tmp_value jsonb;
BEGIN
    tmp_value = retval #> location;
    IF tmp_value IS NOT NULL AND jsonb_typeof(tmp_value) = 'array' THEN
        CASE
            WHEN NOT if_not_exists THEN
                retval = jsonb_set(retval, location, tmp_value || val, FALSE);
            WHEN patch_expression IS NULL AND jsonb_typeof(val) = 'object' AND NOT tmp_value @> jsonb_build_array(val) THEN
                retval = jsonb_set(retval, location, tmp_value || val, FALSE);
            WHEN patch_expression IS NULL AND jsonb_typeof(val) <> 'object' AND NOT tmp_value @> val THEN
                retval = jsonb_set(retval, location, tmp_value || val, FALSE);
            WHEN patch_expression IS NOT NULL AND jsonb_typeof(patch_expression) = 'array' AND jsonb_array_length(patch_expression) = 0 THEN
                retval = jsonb_set(retval, location, tmp_value || val, FALSE);
            ELSE NULL;
            END CASE;
    END IF;
    RETURN retval;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_copy(jsonb, text[], text[])
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    src_path ALIAS FOR $2;
    dst_path ALIAS FOR $3;
    tmp_value jsonb;
BEGIN
    tmp_value = retval #> src_path;
    retval = photos.mt_jsonb_fix_null_parent(retval, dst_path);
    RETURN jsonb_set(retval, dst_path, tmp_value::jsonb, TRUE);
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_duplicate(jsonb, text[], jsonb)
RETURNS jsonb
LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    location ALIAS FOR $2;
    targets ALIAS FOR $3;
    tmp_value jsonb;
    target_path text[];
    target text;
BEGIN
    FOR target IN SELECT jsonb_array_elements_text(targets)
    LOOP
        target_path = photos.mt_jsonb_path_to_array(target, '\.');
        retval = photos.mt_jsonb_copy(retval, location, target_path);
    END LOOP;

    RETURN retval;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_fix_null_parent(jsonb, text[])
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
retval ALIAS FOR $1;
    dst_path ALIAS FOR $2;
    dst_path_segment text[] = ARRAY[]::text[];
    dst_path_array_length integer;
    i integer = 1;
BEGIN
    dst_path_array_length = array_length(dst_path, 1);
    WHILE i <=(dst_path_array_length - 1)
    LOOP
        dst_path_segment = dst_path_segment || ARRAY[dst_path[i]];
        IF retval #> dst_path_segment = 'null'::jsonb THEN
            retval = jsonb_set(retval, dst_path_segment, '{}'::jsonb, TRUE);
        END IF;
        i = i + 1;
    END LOOP;

    RETURN retval;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_increment(jsonb, text[], numeric)
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
retval ALIAS FOR $1;
    location ALIAS FOR $2;
    increment_value ALIAS FOR $3;
    tmp_value jsonb;
BEGIN
    tmp_value = retval #> location;
    IF tmp_value IS NULL THEN
        tmp_value = to_jsonb(0);
END IF;

RETURN jsonb_set(retval, location, to_jsonb(tmp_value::numeric + increment_value), TRUE);
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_insert(jsonb, text[], jsonb, integer, boolean, jsonb default null::jsonb)
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    location ALIAS FOR $2;
    val ALIAS FOR $3;
    elm_index ALIAS FOR $4;
    if_not_exists ALIAS FOR $5;
    patch_expression ALIAS FOR $6;
    tmp_value jsonb;
BEGIN
    tmp_value = retval #> location;
    IF tmp_value IS NOT NULL AND jsonb_typeof(tmp_value) = 'array' THEN
        IF elm_index IS NULL THEN
            elm_index = jsonb_array_length(tmp_value) + 1;
        END IF;
        CASE
            WHEN NOT if_not_exists THEN
                retval = jsonb_insert(retval, location || elm_index::text, val);
            WHEN patch_expression IS NULL AND jsonb_typeof(val) = 'object' AND NOT tmp_value @> jsonb_build_array(val) THEN
                retval = jsonb_insert(retval, location || elm_index::text, val);
            WHEN patch_expression IS NULL AND jsonb_typeof(val) <> 'object' AND NOT tmp_value @> val THEN
                retval = jsonb_insert(retval, location || elm_index::text, val);
            WHEN patch_expression IS NOT NULL AND jsonb_typeof(patch_expression) = 'array' AND jsonb_array_length(patch_expression) = 0 THEN
                retval = jsonb_insert(retval, location || elm_index::text, val);
            ELSE NULL;
        END CASE;
    END IF;
    RETURN retval;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_move(jsonb, text[], text)
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    src_path ALIAS FOR $2;
    dst_name ALIAS FOR $3;
    dst_path text[];
    tmp_value jsonb;
BEGIN
    tmp_value = retval #> src_path;
    retval = retval #- src_path;
    dst_path = src_path;
    dst_path[array_length(dst_path, 1)] = dst_name;
    retval = photos.mt_jsonb_fix_null_parent(retval, dst_path);
    RETURN jsonb_set(retval, dst_path, tmp_value, TRUE);
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_path_to_array(text, character)
    RETURNS text[]
    LANGUAGE plpgsql
AS $function$
DECLARE
    location ALIAS FOR $1;
    regex_pattern ALIAS FOR $2;
BEGIN
RETURN regexp_split_to_array(location, regex_pattern)::text[];
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_remove(jsonb, text[], jsonb)
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    location ALIAS FOR $2;
    val ALIAS FOR $3;
    tmp_value jsonb;
    tmp_remove jsonb;
    patch_remove jsonb;
BEGIN
    tmp_value = retval #> location;
    IF tmp_value IS NOT NULL AND jsonb_typeof(tmp_value) = 'array' THEN
        IF jsonb_typeof(val) = 'array' THEN
            tmp_remove = val;
        ELSE
            tmp_remove = jsonb_build_array(val);
        END IF;

        FOR patch_remove IN SELECT * FROM jsonb_array_elements(tmp_remove)
        LOOP
            tmp_value =(SELECT jsonb_agg(elem)
            FROM jsonb_array_elements(tmp_value) AS elem
            WHERE elem <> patch_remove);
        END LOOP;

        IF tmp_value IS NULL THEN
            tmp_value = '[]'::jsonb;
        END IF;
    END IF;
    RETURN jsonb_set(retval, location, tmp_value, FALSE);
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_jsonb_patch(jsonb, jsonb)
    RETURNS jsonb
    LANGUAGE plpgsql
AS $function$
DECLARE
    retval ALIAS FOR $1;
    patchset ALIAS FOR $2;
    patch jsonb;
    patch_path text[];
    patch_expression jsonb;
    value jsonb;
BEGIN
    FOR patch IN SELECT * from jsonb_array_elements(patchset)
    LOOP
        patch_path = photos.mt_jsonb_path_to_array((patch->>'path')::text, '\.');

        patch_expression = null;
        IF (patch->>'type') IN ('remove', 'append_if_not_exists', 'insert_if_not_exists') AND (patch->>'expression') IS NOT NULL THEN
            patch_expression = jsonb_path_query_array(retval #> patch_path, (patch->>'expression')::jsonpath);
        END IF;

        CASE patch->>'type'
            WHEN 'set' THEN
                retval = jsonb_set(retval, patch_path, (patch->'value')::jsonb, TRUE);
            WHEN 'delete' THEN
                retval = retval#-patch_path;
            WHEN 'append' THEN
                retval = photos.mt_jsonb_append(retval, patch_path, (patch->'value')::jsonb, FALSE);
            WHEN 'append_if_not_exists' THEN
                retval = photos.mt_jsonb_append(retval, patch_path, (patch->'value')::jsonb, TRUE, patch_expression);
            WHEN 'insert' THEN
                retval = photos.mt_jsonb_insert(retval, patch_path, (patch->'value')::jsonb, (patch->>'index')::integer, FALSE);
            WHEN 'insert_if_not_exists' THEN
                retval = photos.mt_jsonb_insert(retval, patch_path, (patch->'value')::jsonb, (patch->>'index')::integer, TRUE, patch_expression);
            WHEN 'remove' THEN
                retval = photos.mt_jsonb_remove(retval, patch_path, COALESCE(patch_expression, (patch->'value')::jsonb));
            WHEN 'duplicate' THEN
                retval = photos.mt_jsonb_duplicate(retval, patch_path, (patch->'targets')::jsonb);
            WHEN 'rename' THEN
                retval = photos.mt_jsonb_move(retval, patch_path, (patch->>'to')::text);
            WHEN 'increment' THEN
                retval = photos.mt_jsonb_increment(retval, patch_path, (patch->>'increment')::numeric);
            WHEN 'increment_float' THEN
                retval = photos.mt_jsonb_increment(retval, patch_path, (patch->>'increment')::numeric);
            ELSE NULL;
        END CASE;
    END LOOP;
    RETURN retval;
END;
$function$;


DROP TABLE IF EXISTS photos.mt_doc_album CASCADE;
CREATE TABLE photos.mt_doc_album (
    id                  varchar                     NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    user_id             uuid                        NULL,
CONSTRAINT pkey_mt_doc_album_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_album_idx_id ON photos.mt_doc_album USING btree ((id));

CREATE INDEX mt_doc_album_idx_user_id ON photos.mt_doc_album USING btree (user_id);

CREATE OR REPLACE FUNCTION photos.mt_upsert_album(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_album ("user_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_user_id, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id)
  DO UPDATE SET "user_id" = arg_user_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_album into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_insert_album(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO photos.mt_doc_album ("user_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_user_id, doc, docDotNetType, docId, docVersion, transaction_timestamp());

  RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION photos.mt_update_album(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
  UPDATE photos.mt_doc_album SET "user_id" = arg_user_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

  SELECT mt_version FROM photos.mt_doc_album into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

alter table photos.mt_doc_objectproperties drop constraint pkey_mt_doc_objectproperties_id CASCADE;
alter table photos.mt_doc_objectproperties add CONSTRAINT pkey_mt_doc_objectproperties_id_mt_deleted PRIMARY KEY (id, mt_deleted);
create table photos.mt_doc_objectproperties_temp as select * from photos.mt_doc_objectproperties;
drop table photos.mt_doc_objectproperties cascade;
DROP TABLE IF EXISTS photos.mt_doc_objectproperties CASCADE;
CREATE TABLE photos.mt_doc_objectproperties (
    id                  varchar                     NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    user_id             uuid                        NULL,
    mt_deleted          boolean                     NOT NULL DEFAULT FALSE,
    mt_deleted_at       timestamp with time zone    NULL,
CONSTRAINT pkey_mt_doc_objectproperties_id_mt_deleted PRIMARY KEY (id, mt_deleted)
) PARTITION BY LIST (mt_deleted);

CREATE INDEX mt_doc_objectproperties_idx_mt_deleted ON photos.mt_doc_objectproperties USING btree (mt_deleted);

CREATE INDEX mt_doc_objectproperties_idx_mt_deleted_at ON photos.mt_doc_objectproperties USING btree (mt_deleted_at) WHERE (mt_deleted);

CREATE INDEX mt_doc_objectproperties_idx_hash ON photos.mt_doc_objectproperties USING btree ((data ->> 'Hash'));

CREATE INDEX mt_doc_objectproperties_idx_user_id ON photos.mt_doc_objectproperties USING btree (user_id);

CREATE TABLE photos.mt_doc_objectproperties_deleted partition of photos.mt_doc_objectproperties for values in (true);

CREATE TABLE photos.mt_doc_objectproperties_default PARTITION OF photos.mt_doc_objectproperties DEFAULT;

insert into photos.mt_doc_objectproperties(id, data, mt_last_modified, mt_version, mt_dotnet_type, user_id, mt_deleted, mt_deleted_at) select id, data, mt_last_modified, mt_version, mt_dotnet_type, user_id, mt_deleted, mt_deleted_at from photos.mt_doc_objectproperties_temp;
drop table photos.mt_doc_objectproperties_temp cascade;
DROP FUNCTION IF EXISTS photos.mt_upsert_objectproperties(arg_user_id uuid, doc jsonb, docdotnettype character varying, docid character varying, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION photos.mt_upsert_objectproperties(arg_user_id uuid, doc JSONB, docDotNetType varchar, docId varchar, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_objectproperties ("user_id", "data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (arg_user_id, doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id, mt_deleted)
  DO UPDATE SET "user_id" = arg_user_id, "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_objectproperties into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

alter table photos.mt_doc_user alter column mt_deleted_at type timestamp with time zone;
CREATE INDEX mt_doc_user_idx_mt_deleted ON photos.mt_doc_user USING btree (mt_deleted);
CREATE INDEX mt_doc_user_idx_mt_deleted_at ON photos.mt_doc_user USING btree (mt_deleted_at) WHERE (mt_deleted);
alter table photos.mt_doc_user drop constraint pkey_mt_doc_user_id CASCADE;
alter table photos.mt_doc_user add CONSTRAINT pkey_mt_doc_user_id_mt_deleted PRIMARY KEY (id, mt_deleted);
create table photos.mt_doc_user_temp as select * from photos.mt_doc_user;
drop table photos.mt_doc_user cascade;
DROP TABLE IF EXISTS photos.mt_doc_user CASCADE;
CREATE TABLE photos.mt_doc_user (
    id                  uuid                        NOT NULL,
    data                jsonb                       NOT NULL,
    mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
    mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
    mt_dotnet_type      varchar                     NULL,
    mt_deleted          boolean                     NOT NULL DEFAULT FALSE,
    mt_deleted_at       timestamp with time zone    NULL,
CONSTRAINT pkey_mt_doc_user_id_mt_deleted PRIMARY KEY (id, mt_deleted)
) PARTITION BY LIST (mt_deleted);

CREATE INDEX mt_doc_user_idx_mt_deleted ON photos.mt_doc_user USING btree (mt_deleted);

CREATE INDEX mt_doc_user_idx_mt_deleted_at ON photos.mt_doc_user USING btree (mt_deleted_at) WHERE (mt_deleted);

CREATE INDEX mt_doc_user_idx_id ON photos.mt_doc_user USING btree ((id));

CREATE TABLE photos.mt_doc_user_deleted partition of photos.mt_doc_user for values in (true);

CREATE TABLE photos.mt_doc_user_default PARTITION OF photos.mt_doc_user DEFAULT;

insert into photos.mt_doc_user(id, data, mt_last_modified, mt_version, mt_dotnet_type, mt_deleted, mt_deleted_at) select id, data, mt_last_modified, mt_version, mt_dotnet_type, mt_deleted, mt_deleted_at from photos.mt_doc_user_temp;
drop table photos.mt_doc_user_temp cascade;
DROP FUNCTION IF EXISTS photos.mt_upsert_user(doc jsonb, docdotnettype character varying, docid uuid, docversion uuid) cascade;

CREATE OR REPLACE FUNCTION photos.mt_upsert_user(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
  final_version uuid;
BEGIN
INSERT INTO photos.mt_doc_user ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
  ON CONFLICT (id, mt_deleted)
  DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

  SELECT mt_version FROM photos.mt_doc_user into final_version WHERE id = docId ;
  RETURN final_version;
END;
$function$;

drop index if exists photos.pk_mt_events_id_unique;

CREATE OR REPLACE FUNCTION photos.mt_quick_append_events(stream uuid, stream_type varchar, tenantid varchar, event_ids uuid[], event_types varchar[], dotnet_types varchar[], bodies jsonb[]) RETURNS int[] AS $$
DECLARE
	event_version int;
	event_type varchar;
	event_id uuid;
	body jsonb;
	index int;
	seq int;
    actual_tenant varchar;
	return_value int[];
BEGIN
	select version into event_version from photos.mt_streams where id = stream;
	if event_version IS NULL then
		event_version = 0;
		insert into photos.mt_streams (id, type, version, timestamp, tenant_id) values (stream, stream_type, 0, now(), tenantid);
    else
        if tenantid IS NOT NULL then
            select tenant_id into actual_tenant from photos.mt_streams where id = stream;
            if actual_tenant != tenantid then
                RAISE EXCEPTION 'The tenantid does not match the existing stream';
            end if;
        end if;
	end if;

	index := 1;
	return_value := ARRAY[event_version + array_length(event_ids, 1)];

	foreach event_id in ARRAY event_ids
	loop
	    seq := nextval('photos.mt_events_sequence');
		return_value := array_append(return_value, seq);

	    event_version := event_version + 1;
		event_type = event_types[index];
		body = bodies[index];

		insert into photos.mt_events
			(seq_id, id, stream_id, version, data, type, tenant_id, timestamp, mt_dotnet_type, is_archived)
		values
			(seq, event_id, stream, event_version, body, event_type, tenantid, (now() at time zone 'utc'), dotnet_types[index], FALSE);

		index := index + 1;
	end loop;

	update photos.mt_streams set version = event_version, timestamp = now() where id = stream;

	return return_value;
END
$$ LANGUAGE plpgsql;

