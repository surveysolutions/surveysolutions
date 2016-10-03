DO language plpgsql $$
DECLARE rscount integer DEFAULT 0;
DECLARE pscount integer DEFAULT 0;
DECLARE next_hi_rs integer DEFAULT 0;
DECLARE next_hi_ps integer DEFAULT 0;
BEGIN
RAISE NOTICE 'migration started';
RAISE NOTICE 'intalling DBLINK';
DROP EXTENSION IF EXISTS dblink;
CREATE EXTENSION dblink;
RAISE NOTICE 'DBLINK intalled';

RAISE NOTICE 'dropping tables';
DROP TABLE IF EXISTS public.sharedpersons, 
		     public.questionnairelistviewitems, 
		     public.questionnairestatetrackers, 
		     public.questionnairechangereferences, 
		     public.questionnairechangerecords, 
		     public.questionnairedocuments, 
		     public.questionnairesharedpersons;
	   
RAISE NOTICE 'tables dropped';

RAISE NOTICE 'creating tables';
CREATE TABLE public.questionnairelistviewitems
(
  id character varying(255) NOT NULL,
  creationdate timestamp without time zone,
  publicid uuid,
  lastentrydate timestamp without time zone,
  title text,
  createdby uuid,
  creatorname text,
  isdeleted boolean,
  ispublic boolean,
  owner text,
  CONSTRAINT questionnairelistviewitems_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.questionnairelistviewitems
  OWNER TO postgres;

CREATE TABLE public.sharedpersons
(
  questionnaireid character varying(255) NOT NULL,
  sharedpersonid uuid,
  CONSTRAINT fkb30c132dd7a23f3 FOREIGN KEY (questionnaireid)
      REFERENCES public.questionnairelistviewitems (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT fkb47d3b5b772857dc FOREIGN KEY (questionnaireid)
      REFERENCES public.questionnairelistviewitems (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.sharedpersons
  OWNER TO postgres;

CREATE INDEX questionnairelistviewitem_sharedpersons
  ON public.sharedpersons
  USING btree
  (questionnaireid COLLATE pg_catalog."default");

  CREATE TABLE public.questionnairesharedpersons
(
  id text NOT NULL,
  value json NOT NULL,
  CONSTRAINT questionnairesharedpersons_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.questionnairesharedpersons
  OWNER TO postgres;

  CREATE TABLE public.questionnairestatetrackers
(
  id text NOT NULL,
  value json NOT NULL,
  CONSTRAINT questionnairestatetrackers_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.questionnairestatetrackers
  OWNER TO postgres;

CREATE TABLE public.questionnairechangerecords
(
  id character varying(255) NOT NULL,
  questionnaireid text,
  userid uuid,
  username text,
  "timestamp" timestamp without time zone,
  sequence integer,
  actiontype integer,
  targetitemtype integer,
  targetitemid uuid,
  targetitemtitle text,
  CONSTRAINT questionnairechangerecords_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.questionnairechangerecords
  OWNER TO postgres;

CREATE INDEX questionnairechangerecord_userid
  ON public.questionnairechangerecords
  USING btree
  (userid);


CREATE INDEX questionnairechangerecord_username
  ON public.questionnairechangerecords
  USING btree
  (username COLLATE pg_catalog."default");

CREATE TABLE public.questionnairechangereferences
(
  id integer NOT NULL,
  referencetype integer,
  referenceid uuid,
  referencetitle text,
  questionnairechangerecordid character varying(255),
  CONSTRAINT questionnairechangereferences_pkey PRIMARY KEY (id),      
  CONSTRAINT fkc4dca6594fec074b FOREIGN KEY (questionnairechangerecordid)
      REFERENCES public.questionnairechangerecords (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION  
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.questionnairechangereferences
  OWNER TO postgres;

CREATE INDEX questionnairechangerecords_questionnairechangereferences
  ON public.questionnairechangereferences
  USING btree
  (questionnairechangerecordid COLLATE pg_catalog."default");

CREATE TABLE public.questionnairedocuments
(
  id text NOT NULL,
  value json NOT NULL,
  CONSTRAINT questionnairedocuments_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.questionnairedocuments
  OWNER TO postgres;

 RAISE NOTICE 'tables created';

RAISE NOTICE 'connecting to read side database';
PERFORM  dblink_connect('views','host=127.0.0.1 port=5432 dbname=Design-Views user=postgres password=Qwerty1234');
RAISE NOTICE 'connected';

RAISE NOTICE 'moving questionnairelistviewitems';
insert into public.questionnairelistviewitems select (rec).* from dblink('views','select t1 from
  questionnairelistviewitems t1') t2 (rec questionnairelistviewitems);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from questionnairelistviewitems t1') t2 (rec questionnairelistviewitems)) as foo);
pscount := (select count(*) from public.questionnairelistviewitems);
RAISE NOTICE 'questionnairelistviewitems moved. Read side count:%, Plain Storage count: %', rscount, pscount;

  RAISE NOTICE 'moving questionnairesharedpersons';
 insert into public.questionnairesharedpersons select (rec).* from dblink('views','select t1 from
  questionnairesharedpersons t1') t2 (rec questionnairesharedpersons);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from questionnairesharedpersons t1') t2 (rec questionnairesharedpersons)) as foo);
pscount := (select count(*) from public.questionnairesharedpersons);
RAISE NOTICE 'questionnairesharedpersons moved. Read side count:%, Plain Storage count: %', rscount, pscount;

RAISE NOTICE 'moving questionnairedocuments';
insert into public.questionnairedocuments select (rec).* from dblink('views','select t1 from
  questionnairedocuments t1') t2 (rec questionnairedocuments);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from questionnairedocuments t1') t2 (rec questionnairedocuments)) as foo);
pscount := (select count(*) from public.questionnairedocuments);
RAISE NOTICE 'questionnairedocuments moved. Read side count:%, Plain Storage count: %', rscount, pscount;

RAISE NOTICE 'moving questionnairestatetrackers';
 insert into public.questionnairestatetrackers select (rec).* from dblink('views','select t1 from
  questionnairestatetrackers t1') t2 (rec questionnairestatetrackers);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from questionnairestatetrackers t1') t2 (rec questionnairestatetrackers)) as foo);
pscount := (select count(*) from public.questionnairestatetrackers);
RAISE NOTICE 'questionnairestatetrackers moved. Read side count:%, Plain Storage count: %', rscount, pscount;

RAISE NOTICE 'moving questionnairechangerecords';
  insert into public.questionnairechangerecords select (rec).* from dblink('views','select t1 from
  questionnairechangerecords t1') t2 (rec questionnairechangerecords);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from questionnairechangerecords t1') t2 (rec questionnairechangerecords)) as foo);
pscount := (select count(*) from public.questionnairechangerecords);
RAISE NOTICE 'questionnairechangerecords moved. Read side count:%, Plain Storage count: %', rscount, pscount;

  RAISE NOTICE 'moving questionnairechangereferences';
  insert into public.questionnairechangereferences select (rec).* from dblink('views','select t1 from
  questionnairechangereferences t1') t2 (rec questionnairechangereferences);

  rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from questionnairechangereferences t1') t2 (rec questionnairechangereferences)) as foo);
pscount := (select count(*) from public.questionnairechangereferences);
RAISE NOTICE 'questionnairechangereferences moved. Read side count:%, Plain Storage count: %', rscount, pscount;

  RAISE NOTICE 'moving sharedpersons';
  insert into public.sharedpersons select (rec).* from dblink('views','select t1 from
  sharedpersons t1') t2 (rec sharedpersons);
  
  rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from sharedpersons t1') t2 (rec sharedpersons)) as foo);
pscount := (select count(*) from public.sharedpersons);
RAISE NOTICE 'sharedpersons moved. Read side count:%, Plain Storage count: %', rscount, pscount;


next_hi_rs := (select next_hi from (select (rec).* from dblink('views','select t1 from hibernate_unique_key t1') t2 (rec hibernate_unique_key)) as foo);
next_hi_ps := (select next_hi from public.hibernate_unique_key);
RAISE NOTICE 'next_hi_rs values. Read side:%, Plain Storage: %', next_hi_rs, next_hi_ps;
IF next_hi_rs > next_hi_ps THEN
  UPDATE public.hibernate_unique_key SET next_hi=next_hi_rs;
  RAISE NOTICE 'updated next_hi value for read side to :%', next_hi_rs;
END IF;


PERFORM dblink_disconnect('views');
RAISE NOTICE 'migration finished';
END
$$;
--select dblink_disconnect('views');