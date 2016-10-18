CREATE TABLE IF NOT EXISTS @schemaName.events
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

CREATE INDEX IF NOT EXISTS event_source_indx
  ON @schemaName.events
  USING btree
  (eventsourceid);

CREATE INDEX IF NOT EXISTS globalsequence_indx
  ON @schemaName.events
  USING btree
  (globalsequence);

CREATE UNIQUE INDEX IF NOT EXISTS event_source_eventsequence_indx
  ON @schemaName.events
  USING btree
  (eventsourceid, eventsequence);
