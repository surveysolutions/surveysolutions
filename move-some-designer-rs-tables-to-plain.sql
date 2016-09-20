CREATE EXTENSION dblink;

--DROP TABLE public.sharedpersons;
--DROP TABLE public.questionnairelistviewitems;
--DROP TABLE public.questionnairestatetrackers;
--DROP TABLE public.questionnairechangereferences;
--DROP TABLE public.questionnairechangerecords;
--DROP TABLE public.questionnairedocuments;
--DROP TABLE public.questionnairesharedpersons;


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
  questionnairechangerecord character varying(255),
  questionnairechangerecordid character varying(255),
  CONSTRAINT questionnairechangereferences_pkey PRIMARY KEY (id),
  CONSTRAINT fk1d5d002f19131125 FOREIGN KEY (questionnairechangerecord)
      REFERENCES public.questionnairechangerecords (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT fk1d5d002f594ea6ac FOREIGN KEY (questionnairechangerecordid)
      REFERENCES public.questionnairechangerecords (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT fkc4dca6594fec074b FOREIGN KEY (questionnairechangerecordid)
      REFERENCES public.questionnairechangerecords (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION,
  CONSTRAINT fkc4dca659fdf34748 FOREIGN KEY (questionnairechangerecord)
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
  (questionnairechangerecord COLLATE pg_catalog."default");


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

select dblink_connect('views','host=127.0.0.1 port=5432 dbname=Design-Views user=postgres password=P@$$w0rd');

insert into public.questionnairelistviewitems select (rec).* from dblink('views','select t1 from
  questionnairelistviewitems t1') t2 (rec questionnairelistviewitems);
  
 insert into public.questionnairesharedpersons select (rec).* from dblink('views','select t1 from
  questionnairesharedpersons t1') t2 (rec questionnairesharedpersons);

insert into public.questionnairedocuments select (rec).* from dblink('views','select t1 from
  questionnairedocuments t1') t2 (rec questionnairedocuments);

 insert into public.questionnairestatetrackers select (rec).* from dblink('views','select t1 from
  questionnairestatetrackers t1') t2 (rec questionnairestatetrackers);

  insert into public.questionnairechangerecords select (rec).* from dblink('views','select t1 from
  questionnairechangerecords t1') t2 (rec questionnairechangerecords);
  
  insert into public.questionnairechangereferences select (rec).* from dblink('views','select t1 from
  questionnairechangereferences t1') t2 (rec questionnairechangereferences);
  
  insert into public.sharedpersons select (rec).* from dblink('views','select t1 from
  sharedpersons t1') t2 (rec sharedpersons);

select dblink_disconnect('views');
  