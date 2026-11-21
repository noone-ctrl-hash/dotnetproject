-- À exécuter une seule fois avec un compte PostgreSQL administrateur (ex. : psql -U postgres -f database/setup.sql)
CREATE USER biblio WITH PASSWORD 'biblio_pwd';
CREATE DATABASE bibliotheque OWNER biblio;
