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

--------------------------------------------------------------

RAISE NOTICE 'dropping tables';

DROP TABLE IF EXISTS public.users, 
		     public.simpleroles;

RAISE NOTICE 'tables dropped';

--------------------------------------------------------------

RAISE NOTICE 'creating tables';

-- Table: public.users

-- DROP TABLE public.users;

CREATE TABLE public.users
(
  id character varying(255) NOT NULL,
  applicationname text,
  comment text,
  confirmationtoken text,
  createdat timestamp without time zone,
  email text,
  isconfirmed boolean,
  islockedout boolean,
  isonline boolean,
  lastactivityat timestamp without time zone,
  lastlockedoutat timestamp without time zone,
  lastloginat timestamp without time zone,
  lastpasswordchangeat timestamp without time zone,
  password text,
  passwordanswer text,
  passwordquestion text,
  passwordresetexpirationdate timestamp without time zone,
  passwordresettoken text,
  passwordsalt text,
  provideruserkey uuid,
  username text,
  CONSTRAINT "PK_users" PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.users
  OWNER TO postgres;

-- Table: public.simpleroles

-- DROP TABLE public.simpleroles;

CREATE TABLE public.simpleroles
(
  userid character varying(255) NOT NULL,
  simpleroleid integer,
  CONSTRAINT "FK_simpleroles_userid_users_id" FOREIGN KEY (userid)
      REFERENCES public.users (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.simpleroles
  OWNER TO postgres;

-- Index: public.simpleroles_userid_indx

-- DROP INDEX public.simpleroles_userid_indx;

CREATE INDEX simpleroles_userid_indx
  ON public.simpleroles
  USING btree
  (userid COLLATE pg_catalog."default");

RAISE NOTICE 'tables created';

--------------------------------------------------------------

RAISE NOTICE 'connecting to read side database';
--PERFORM dblink_connect('views','host=127.0.0.1 port=5432 dbname=Design-Views user=postgres password=Qwerty1234');
PERFORM dblink_connect('views','host=127.0.0.1 port=5432 dbname=design-dev-Views user=postgres password=Qwerty1234');
--PERFORM dblink_connect('views','host=127.0.0.1 port=5432 dbname=D.Local.ReadSide user=postgres password=Qwerty1234');
RAISE NOTICE 'connected';

--------------------------------------------------------------

RAISE NOTICE 'moving accountdocuments to users';
insert into public.users select (rec).* from dblink('views','select t1 from accountdocuments t1') t2 (rec users);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from accountdocuments t1') t2 (rec users)) as foo);
pscount := (select count(*) from public.users);
RAISE NOTICE 'users moved. read side count: %, plain count: %', rscount, pscount;

--------------------------------------------------------------

RAISE NOTICE 'moving simpleroles';
insert into public.simpleroles select (rec).* from dblink('views','select t1 from simpleroles t1') t2 (rec simpleroles);

rscount := (select count(*) from (select (rec).* from dblink('views','select t1 from simpleroles t1') t2 (rec simpleroles)) as foo);
pscount := (select count(*) from public.simpleroles);
RAISE NOTICE 'simpleroles moved. read side count: %, plain count: %', rscount, pscount;

--------------------------------------------------------------

RAISE NOTICE 'updating NHibernate values';
next_hi_rs := (select next_hi from (select (rec).* from dblink('views','select t1 from hibernate_unique_key t1') t2 (rec hibernate_unique_key)) as foo);
next_hi_ps := (select next_hi from public.hibernate_unique_key);
RAISE NOTICE 'next_hi values. Read Side: %, Plain: %', next_hi_rs, next_hi_ps;
IF next_hi_rs > next_hi_ps THEN
  UPDATE public.hibernate_unique_key SET next_hi=next_hi_rs;
  RAISE NOTICE 'updated next_hi value for read side to :%', next_hi_rs;
END IF;
RAISE NOTICE 'updated NHibernate values (if Read Side <= Plain then nothing was changed)';

--------------------------------------------------------------

RAISE NOTICE 'disconnecting from read side database';
PERFORM dblink_disconnect('views');
RAISE NOTICE 'disconnected';

--------------------------------------------------------------

RAISE NOTICE 'migration finished';
END
$$;
