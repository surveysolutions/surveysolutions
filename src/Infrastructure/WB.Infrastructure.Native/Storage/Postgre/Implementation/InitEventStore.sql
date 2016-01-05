CREATE TABLE events
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

CREATE INDEX event_source_indx
  ON events
  USING btree
  (eventsourceid);

