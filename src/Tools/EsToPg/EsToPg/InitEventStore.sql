CREATE TABLE IF NOT EXISTS events
(
  id uuid NOT NULL,
  origin text,
  "timestamp" timestamp without time zone NOT NULL,
  eventsourceid uuid NOT NULL,
  globalsequence integer NOT NULL,
  value json NOT NULL,
  eventsequence integer NOT NULL,
  eventtype text NOT NULL,
  CONSTRAINT pk PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);

DO $$
BEGIN

IF NOT EXISTS (
    SELECT 1
    FROM   pg_class c
    JOIN   pg_namespace n ON n.oid = c.relnamespace
    WHERE  c.relname = 'events'
    AND    n.nspname = 'public' -- 'public' by default
    ) THEN

    CREATE INDEX event_source_indx ON events USING btree (eventsourceid);
END IF;

CREATE INDEX globalsequence_indx  ON events USING btree (globalsequence);

END$$;
  

