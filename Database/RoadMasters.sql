--
-- PostgreSQL database dump
--

-- Dumped from database version 15.0
-- Dumped by pg_dump version 15rc2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: changeUserName(character varying, character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."changeUserName"(username character varying, pass character varying, oldusername character varying) RETURNS smallint
    LANGUAGE plpgsql
    AS $$ 
DECLARE
    res SMALLINT;
    checkPass RECORD;
BEGIN
    SELECT "Password" INTO checkPass FROM "User" WHERE "UserName" = oldUsername;
    IF checkPass."Password" = pass THEN
        IF exists(SELECT "UserName" FROM "User" WHERE "UserName" = username) THEN
            res = 2;
        ELSE
            UPDATE "User" SET "UserName" = username WHERE "UserName" = oldUsername;
            res = 1;
        END IF;
    ELSE
        res = 3;
    END IF;
    RETURN res;
END;
$$;


ALTER FUNCTION public."changeUserName"(username character varying, pass character varying, oldusername character varying) OWNER TO postgres;

--
-- Name: deleteMap(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."deleteMap"(mapindex integer) RETURNS smallint
    LANGUAGE plpgsql
    AS $$ 
DECLARE
    res SMALLINT;
BEGIN  
    IF (SELECT AGE(CURRENT_TIMESTAMP, (SELECT "Date" FROM "Map" WHERE "Index" = mapindex)::TIMESTAMP)::INTERVAL < '1 days') THEN
        DELETE FROM "Map" WHERE "Index" = mapindex;
        res = 1;
    ELSE
        res = 2;
    END IF;
    RETURN res;
END;
$$;


ALTER FUNCTION public."deleteMap"(mapindex integer) OWNER TO postgres;

--
-- Name: getObject(character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."getObject"(pointkey character varying) RETURNS character varying
    LANGUAGE plpgsql
    AS $$ 
DECLARE
    status SMALLINT;
    pos LINE;
    speed REAL;
    res CHARACTER VARYING;
BEGIN  
    IF exists(SELECT "Pos" FROM "GameObject" WHERE "PointKey" = pointkey) THEN
        SELECT "Pos" INTO pos FROM "GameObject" WHERE "PointKey" = pointkey;
        IF exists(SELECT "Speed" FROM "MovingObject" WHERE "PointKey" = pointkey) THEN
            SELECT "Speed" INTO speed FROM "MovingObject" WHERE "PointKey" = pointkey;
            status = 2;
        ELSE
            status = 1;
            speed = 0;
        END IF;
    ELSE
        status = 0;
        speed = 0;
        SELECT "Pos" INTO pos FROM "Point" WHERE "Key" = pointkey;
    END IF;
    res = status || '@' || pos || '@' || speed;
    RETURN res;
END;
$$;


ALTER FUNCTION public."getObject"(pointkey character varying) OWNER TO postgres;

--
-- Name: logAddedMaps(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logAddedMaps"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
IF EXISTS(SELECT "Creator" FROM "BestCreators" WHERE "Creator" = NEW."Creator") THEN
UPDATE "BestCreators" SET "MapCount" = ("MapCount" + 1)::INTEGER WHERE "Creator" = NEW."Creator";
ELSE
INSERT INTO "BestCreators" ("Creator", "MapCount") VALUES (NEW."Creator", 1);
END IF;
RETURN NEW;
END;
$$;


ALTER FUNCTION public."logAddedMaps"() OWNER TO postgres;

--
-- Name: logDeletedMaps(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logDeletedMaps"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    count INTEGER;
BEGIN
INSERT INTO "DeletedMaps" ("Creator", "Index", "Name", "Date") VALUES (OLD."Creator", OLD."Index", OLD."Name", CURRENT_TIMESTAMP);
SELECT "MapCount" INTO count FROM "BestCreators" WHERE "Creator" = OLD."Creator";
IF count = 1 THEN
DELETE FROM "BestCreators" WHERE "Creator" = OLD."Creator";
ELSE
UPDATE "BestCreators" SET "MapCount" = ("MapCount" - 1)::INTEGER WHERE "Creator" = OLD."Creator";
END IF;
RETURN NEW;
END;
$$;


ALTER FUNCTION public."logDeletedMaps"() OWNER TO postgres;

--
-- Name: logUser(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logUser"(username character varying, pass character varying) RETURNS smallint
    LANGUAGE plpgsql
    AS $$ 
DECLARE
    res SMALLINT;
    checkPass RECORD;
BEGIN
    SELECT "Password" INTO checkPass FROM "User" WHERE "UserName" = username;
    IF checkPass."Password" = pass THEN
        res = 1;
        INSERT INTO "LoginHistory" ("UserName", "LoginTime") VALUES (username, CURRENT_TIMESTAMP);
    ELSE
        res = 2;
    END IF;
    RETURN res;
END;
$$;


ALTER FUNCTION public."logUser"(username character varying, pass character varying) OWNER TO postgres;

--
-- Name: logUserHistory(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logUserHistory"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
INSERT INTO "LoginHistory" ("UserName", "LoginTime") VALUES (NEW."UserName", CURRENT_TIMESTAMP);
RETURN NEW;
END;
$$;


ALTER FUNCTION public."logUserHistory"() OWNER TO postgres;

--
-- Name: logUsernameChange(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logUsernameChange"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
INSERT INTO "UserNameHistory" ("UserName", "OldUserName", "Date") VALUES (NEW."UserName", OLD."UserName", CURRENT_TIMESTAMP);
RETURN NEW;
END;
$$;


ALTER FUNCTION public."logUsernameChange"() OWNER TO postgres;

--
-- Name: registerUser(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."registerUser"(username character varying, pass character varying) RETURNS smallint
    LANGUAGE plpgsql
    AS $$ 
DECLARE
    res SMALLINT;
BEGIN  
    IF exists(SELECT "UserName" FROM "User" WHERE "UserName" = username) THEN
        res = 2;
    ELSE
        INSERT INTO "User" ("UserName", "Password") VALUES (username, pass);
        res = 1;
    END IF;
    RETURN res;
END;
$$;


ALTER FUNCTION public."registerUser"(username character varying, pass character varying) OWNER TO postgres;

--
-- Name: setBestTime(double precision, character varying, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."setBestTime"(usertime double precision, username character varying, mapindex integer) RETURNS double precision
    LANGUAGE plpgsql
    AS $$ 
DECLARE
    res DOUBLE PRECISION;
BEGIN  
    IF exists(SELECT "Time" FROM "BestTimes" WHERE "User" = username AND "MapIndex" = mapindex) THEN
        SELECT "Time" INTO res FROM "BestTimes" WHERE "User" = username AND "MapIndex" = mapindex;
        IF usertime < res THEN
            Update "BestTimes" set "Time" = usertime where "User" = username AND "MapIndex" = mapindex;
            res = usertime;
        END IF;
    ELSE
        INSERT INTO "BestTimes" ("MapIndex", "Time", "User") VALUES (mapindex, usertime, username);
        res = usertime;
    END IF;
    RETURN res;
END;
$$;


ALTER FUNCTION public."setBestTime"(usertime double precision, username character varying, mapindex integer) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: BestCreators; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."BestCreators" (
    "Creator" character varying NOT NULL,
    "MapCount" integer NOT NULL
);


ALTER TABLE public."BestCreators" OWNER TO postgres;

--
-- Name: BestTimes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."BestTimes" (
    "Index" integer NOT NULL,
    "User" character varying NOT NULL,
    "MapIndex" integer NOT NULL,
    "Time" double precision NOT NULL
);


ALTER TABLE public."BestTimes" OWNER TO postgres;

--
-- Name: BestTimes_Index_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."BestTimes_Index_seq"
    AS integer
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."BestTimes_Index_seq" OWNER TO postgres;

--
-- Name: BestTimes_Index_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."BestTimes_Index_seq" OWNED BY public."BestTimes"."Index";


--
-- Name: Brake; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Brake" (
    "MapIndex" integer NOT NULL,
    "MaxBrakeTorque" integer NOT NULL,
    "HandBrakeTorque" integer NOT NULL
);


ALTER TABLE public."Brake" OWNER TO postgres;

--
-- Name: DeletedMaps; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."DeletedMaps" (
    "Index" integer NOT NULL,
    "Name" character varying NOT NULL,
    "Creator" character varying NOT NULL,
    "Date" timestamp with time zone NOT NULL
);


ALTER TABLE public."DeletedMaps" OWNER TO postgres;

--
-- Name: Engine; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Engine" (
    "MaxTorque" integer NOT NULL,
    "MaxRpm" integer NOT NULL,
    "MapIndex" integer NOT NULL
);


ALTER TABLE public."Engine" OWNER TO postgres;

--
-- Name: GameObject; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GameObject" (
    "Pos" line NOT NULL,
    "PointKey" character varying NOT NULL
);


ALTER TABLE public."GameObject" OWNER TO postgres;

--
-- Name: GameVersion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GameVersion" (
    "Version" smallint NOT NULL
);


ALTER TABLE public."GameVersion" OWNER TO postgres;

--
-- Name: LoginHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."LoginHistory" (
    "UserName" character varying NOT NULL,
    "LoginTime" timestamp with time zone NOT NULL,
    "Index" integer NOT NULL
);


ALTER TABLE public."LoginHistory" OWNER TO postgres;

--
-- Name: LoginHistory_Index_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."LoginHistory_Index_seq"
    AS integer
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."LoginHistory_Index_seq" OWNER TO postgres;

--
-- Name: LoginHistory_Index_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."LoginHistory_Index_seq" OWNED BY public."LoginHistory"."Index";


--
-- Name: Map; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Map" (
    "Index" integer NOT NULL,
    "Name" character varying NOT NULL,
    "Creator" character varying NOT NULL,
    "Date" timestamp with time zone NOT NULL
);


ALTER TABLE public."Map" OWNER TO postgres;

--
-- Name: Map_Index_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."Map_Index_seq"
    AS integer
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."Map_Index_seq" OWNER TO postgres;

--
-- Name: Map_Index_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."Map_Index_seq" OWNED BY public."Map"."Index";


--
-- Name: MovingObject; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MovingObject" (
    "PointKey" character varying NOT NULL,
    "Speed" real NOT NULL
);


ALTER TABLE public."MovingObject" OWNER TO postgres;

--
-- Name: Point; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Point" (
    "Key" character varying NOT NULL,
    "MapIndex" integer NOT NULL,
    "Pos" line NOT NULL,
    "Index" smallint NOT NULL
);


ALTER TABLE public."Point" OWNER TO postgres;

--
-- Name: RigidBody; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."RigidBody" (
    "MapIndex" integer NOT NULL,
    "Mass" integer NOT NULL
);


ALTER TABLE public."RigidBody" OWNER TO postgres;

--
-- Name: Steering; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Steering" (
    "MapIndex" integer NOT NULL,
    "MaxSteerAngle" integer NOT NULL
);


ALTER TABLE public."Steering" OWNER TO postgres;

--
-- Name: User; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."User" (
    "UserName" character varying NOT NULL,
    "Password" character varying NOT NULL
);


ALTER TABLE public."User" OWNER TO postgres;

--
-- Name: UserNameHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."UserNameHistory" (
    "Index" integer NOT NULL,
    "UserName" character varying NOT NULL,
    "OldUserName" character varying NOT NULL,
    "Date" timestamp with time zone NOT NULL
);


ALTER TABLE public."UserNameHistory" OWNER TO postgres;

--
-- Name: UserNameHistory_Index_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."UserNameHistory_Index_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."UserNameHistory_Index_seq" OWNER TO postgres;

--
-- Name: UserNameHistory_Index_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."UserNameHistory_Index_seq" OWNED BY public."UserNameHistory"."Index";


--
-- Name: BestTimes Index; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestTimes" ALTER COLUMN "Index" SET DEFAULT nextval('public."BestTimes_Index_seq"'::regclass);


--
-- Name: LoginHistory Index; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LoginHistory" ALTER COLUMN "Index" SET DEFAULT nextval('public."LoginHistory_Index_seq"'::regclass);


--
-- Name: Map Index; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Map" ALTER COLUMN "Index" SET DEFAULT nextval('public."Map_Index_seq"'::regclass);


--
-- Name: UserNameHistory Index; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserNameHistory" ALTER COLUMN "Index" SET DEFAULT nextval('public."UserNameHistory_Index_seq"'::regclass);


--
-- Data for Name: BestCreators; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."BestCreators" VALUES
	('admin', 1);


--
-- Data for Name: BestTimes; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."BestTimes" VALUES
	(6, 'admin', 13, 71.1015);


--
-- Data for Name: Brake; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Brake" VALUES
	(13, 3000, 1800);


--
-- Data for Name: DeletedMaps; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."DeletedMaps" VALUES
	(12, 'test', 'admin', '2022-12-15 11:19:49.503193+03');


--
-- Data for Name: Engine; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Engine" VALUES
	(230, 10000, 13);


--
-- Data for Name: GameObject; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."GameObject" VALUES
	('{8.517114,2.3,400.2921}', '13:4'),
	('{2.995146,2.3,500.1707}', '13:5'),
	('{0.0001,2.3,700}', '13:7'),
	('{2.345541,2.3,1300.865}', '13:13'),
	('{0.0001,2.3,1600}', '13:16');


--
-- Data for Name: GameVersion; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."GameVersion" VALUES
	(1);


--
-- Data for Name: LoginHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."LoginHistory" VALUES
	('admin', '2022-12-15 11:02:14.592432+03', 107),
	('admin', '2022-12-15 11:05:08.0252+03', 108),
	('admin', '2022-12-15 11:07:11.236848+03', 109);


--
-- Data for Name: Map; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Map" VALUES
	(13, 'test', 'admin', '2022-12-15 11:21:48.126031+03');


--
-- Data for Name: MovingObject; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."MovingObject" VALUES
	('13:7', 1),
	('13:16', 1);


--
-- Data for Name: Point; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Point" VALUES
	('13:0', 13, '{0.0001,0,0}', 0),
	('13:1', 13, '{0.0001,0,100}', 1),
	('13:2', 13, '{34,0,200}', 2),
	('13:3', 13, '{-24,0,300}', 3),
	('13:4', 13, '{11,0,400}', 4),
	('13:5', 13, '{0.0001,0,500}', 5),
	('13:6', 13, '{0.0001,35,600}', 6),
	('13:7', 13, '{0.0001,0,700}', 7),
	('13:8', 13, '{0.0001,-31,800}', 8),
	('13:9', 13, '{-21,0,900}', 9),
	('13:10', 13, '{33,0,1000}', 10),
	('13:11', 13, '{80,0,1100}', 11),
	('13:12', 13, '{83,0,1200}', 12),
	('13:13', 13, '{0.0001,0,1300}', 13),
	('13:14', 13, '{0.0001,31,1400}', 14),
	('13:15', 13, '{0.0001,-21,1500}', 15),
	('13:16', 13, '{0.0001,0,1600}', 16),
	('13:17', 13, '{0.0001,19,1700}', 17),
	('13:18', 13, '{0.0001,-25,1800}', 18),
	('13:19', 13, '{0.0001,0,1900}', 19);


--
-- Data for Name: RigidBody; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: Steering; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: User; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."User" VALUES
	('admin', '21232f297a57a5a743894a0e4a801fc3');


--
-- Data for Name: UserNameHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Name: BestTimes_Index_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."BestTimes_Index_seq"', 6, true);


--
-- Name: LoginHistory_Index_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."LoginHistory_Index_seq"', 109, true);


--
-- Name: Map_Index_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Map_Index_seq"', 13, true);


--
-- Name: UserNameHistory_Index_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."UserNameHistory_Index_seq"', 2, true);


--
-- Name: BestTimes BestTimes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestTimes"
    ADD CONSTRAINT "BestTimes_pkey" PRIMARY KEY ("Index");


--
-- Name: DeletedMaps DeletedMaps_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DeletedMaps"
    ADD CONSTRAINT "DeletedMaps_pkey" PRIMARY KEY ("Index");


--
-- Name: GameObject GameObject_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GameObject"
    ADD CONSTRAINT "GameObject_pkey" PRIMARY KEY ("PointKey");


--
-- Name: GameVersion GameVersion_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GameVersion"
    ADD CONSTRAINT "GameVersion_pkey" PRIMARY KEY ("Version");


--
-- Name: LoginHistory LoginHistory_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LoginHistory"
    ADD CONSTRAINT "LoginHistory_pkey" PRIMARY KEY ("Index");


--
-- Name: Point Point_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Point"
    ADD CONSTRAINT "Point_pkey" PRIMARY KEY ("Key");


--
-- Name: RigidBody RigidBody_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RigidBody"
    ADD CONSTRAINT "RigidBody_pkey" PRIMARY KEY ("MapIndex");


--
-- Name: Steering Steering_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Steering"
    ADD CONSTRAINT "Steering_pkey" PRIMARY KEY ("MapIndex");


--
-- Name: BestCreators Unique_BestCreators; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestCreators"
    ADD CONSTRAINT "Unique_BestCreators" UNIQUE ("Creator");


--
-- Name: Brake Unique_BrakeMap; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Brake"
    ADD CONSTRAINT "Unique_BrakeMap" UNIQUE ("MapIndex");


--
-- Name: Engine Unique_EngineMap; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Engine"
    ADD CONSTRAINT "Unique_EngineMap" UNIQUE ("MapIndex");


--
-- Name: Map Unique_MapIndex; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Map"
    ADD CONSTRAINT "Unique_MapIndex" UNIQUE ("Index");


--
-- Name: MovingObject Unique_MovingPoint; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MovingObject"
    ADD CONSTRAINT "Unique_MovingPoint" UNIQUE ("PointKey");


--
-- Name: RigidBody Unique_RigidBodyMap; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RigidBody"
    ADD CONSTRAINT "Unique_RigidBodyMap" UNIQUE ("MapIndex");


--
-- Name: User User_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "User_pkey" PRIMARY KEY ("UserName");


--
-- Name: BestCreators unique_BestCreators_Creator; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestCreators"
    ADD CONSTRAINT "unique_BestCreators_Creator" PRIMARY KEY ("Creator");


--
-- Name: Brake unique_Brake_Index; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Brake"
    ADD CONSTRAINT "unique_Brake_Index" PRIMARY KEY ("MapIndex");


--
-- Name: DeletedMaps unique_DeletedMaps_Index; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DeletedMaps"
    ADD CONSTRAINT "unique_DeletedMaps_Index" UNIQUE ("Index");


--
-- Name: Engine unique_Engine_MapIndex; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Engine"
    ADD CONSTRAINT "unique_Engine_MapIndex" PRIMARY KEY ("MapIndex");


--
-- Name: GameObject unique_GameObject_PointKey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GameObject"
    ADD CONSTRAINT "unique_GameObject_PointKey" UNIQUE ("PointKey");


--
-- Name: Map unique_Map_Index; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Map"
    ADD CONSTRAINT "unique_Map_Index" PRIMARY KEY ("Index");


--
-- Name: MovingObject unique_MovingObject_Index; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MovingObject"
    ADD CONSTRAINT "unique_MovingObject_Index" PRIMARY KEY ("PointKey");


--
-- Name: Point unique_Point; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Point"
    ADD CONSTRAINT "unique_Point" UNIQUE ("Key");


--
-- Name: Steering unique_Steering_MapIndex; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Steering"
    ADD CONSTRAINT "unique_Steering_MapIndex" UNIQUE ("MapIndex");


--
-- Name: UserNameHistory unique_UserNameHistory_Index; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserNameHistory"
    ADD CONSTRAINT "unique_UserNameHistory_Index" PRIMARY KEY ("Index");


--
-- Name: User unique_User_field1; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "unique_User_field1" UNIQUE ("UserName");


--
-- Name: Map logAddedMap; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER "logAddedMap" AFTER INSERT ON public."Map" FOR EACH ROW EXECUTE FUNCTION public."logAddedMaps"();


--
-- Name: Map logDeletedMap; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER "logDeletedMap" AFTER DELETE ON public."Map" FOR EACH ROW EXECUTE FUNCTION public."logDeletedMaps"();


--
-- Name: User logFirstLogin; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER "logFirstLogin" AFTER INSERT ON public."User" FOR EACH ROW EXECUTE FUNCTION public."logUserHistory"();


--
-- Name: User logNameChange; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER "logNameChange" AFTER UPDATE ON public."User" FOR EACH ROW EXECUTE FUNCTION public."logUsernameChange"();


--
-- Name: BestTimes MapHasBestTimes; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestTimes"
    ADD CONSTRAINT "MapHasBestTimes" FOREIGN KEY ("MapIndex") REFERENCES public."Map"("Index") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: Brake MapHasBrakeSettings; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Brake"
    ADD CONSTRAINT "MapHasBrakeSettings" FOREIGN KEY ("MapIndex") REFERENCES public."Map"("Index") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: Engine MapHasEngineSettings; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Engine"
    ADD CONSTRAINT "MapHasEngineSettings" FOREIGN KEY ("MapIndex") REFERENCES public."Map"("Index") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: Point MapHasPoints; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Point"
    ADD CONSTRAINT "MapHasPoints" FOREIGN KEY ("MapIndex") REFERENCES public."Map"("Index") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: RigidBody MapHasRigidBodySettings; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RigidBody"
    ADD CONSTRAINT "MapHasRigidBodySettings" FOREIGN KEY ("MapIndex") REFERENCES public."Map"("Index") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: Steering MapHasSteeringSettings; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Steering"
    ADD CONSTRAINT "MapHasSteeringSettings" FOREIGN KEY ("MapIndex") REFERENCES public."Map"("Index") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: MovingObject MovingInheritsObject; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."MovingObject"
    ADD CONSTRAINT "MovingInheritsObject" FOREIGN KEY ("PointKey") REFERENCES public."GameObject"("PointKey") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: GameObject PointHasObject; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GameObject"
    ADD CONSTRAINT "PointHasObject" FOREIGN KEY ("PointKey") REFERENCES public."Point"("Key") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: Map UserCreatesMaps; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Map"
    ADD CONSTRAINT "UserCreatesMaps" FOREIGN KEY ("Creator") REFERENCES public."User"("UserName") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: BestTimes UserHasBestTimes; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestTimes"
    ADD CONSTRAINT "UserHasBestTimes" FOREIGN KEY ("User") REFERENCES public."User"("UserName") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: DeletedMaps UserHasDeletedMaps; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DeletedMaps"
    ADD CONSTRAINT "UserHasDeletedMaps" FOREIGN KEY ("Creator") REFERENCES public."User"("UserName") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: LoginHistory UserHasLoginHistory; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LoginHistory"
    ADD CONSTRAINT "UserHasLoginHistory" FOREIGN KEY ("UserName") REFERENCES public."User"("UserName") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: BestCreators UserHasMapCount; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."BestCreators"
    ADD CONSTRAINT "UserHasMapCount" FOREIGN KEY ("Creator") REFERENCES public."User"("UserName") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: UserNameHistory UserHasNameHistory; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserNameHistory"
    ADD CONSTRAINT "UserHasNameHistory" FOREIGN KEY ("UserName") REFERENCES public."User"("UserName") MATCH FULL ON UPDATE CASCADE ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

