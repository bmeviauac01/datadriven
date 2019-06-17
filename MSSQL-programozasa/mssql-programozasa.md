# Microsoft SQL Server programozása

A példák a minta adatbázison futtathatóak.

## Változók

Változó deklaráció:

```sql
DECLARE @darab int

SELECT @darab
-- NULL
```

Értékadás a `SET` utasítással, vagy a deklarációban:

```sql
DECLARE @darab int = 5

SELECT @darab
-- 4

SET @darab = 3

SELECT @darab
-- 3
```

A változó *nem* kötődik scope-hoz (utasítás blokkhoz). A változó az un. *batch*-en belül vagy tárolt eljáráson belül érvényes:

```sql
BEGIN
  DECLARE @darab int
  SET @darab = 3
END

SELECT @darab
-- Ez működik, a változó az utasítás blokkon kívül is elérhető.
-- 3

GO -- új batch-et kezd

SELECT @darab
-- Hiba: Must declare the scalar variable "@darab".
```

Változó értékadása lekérdezéssel:

```sql
DECLARE @nev nvarchar(max)

SELECT @nev = Nev
FROM Vevo
WHERE ID = 1

SELECT @nev
-- Puskás Norbert
```

Ha a lekérdezés több sorral tér vissza, az *utolsó* érték marad a változóban:

```sql
DECLARE @nev nvarchar(max)

SELECT @nev = Nev
FROM Vevo
-- több illeszkedő sor is lesz

SELECT @nev
-- Grosz János (SELECT utolsó eredménye kerül a változóba)
```

Ha a lekérdezés nem tér vissza eredménnyel, a változó értéke nem változik:

```sql
DECLARE @nev nvarchar(max)
SET @nev = 'alma'

SELECT @nev = Nev
FROM Vevo
WHERE ID = 99999999
-- nincs illeszkedő sor

SELECT @nev
-- alma
```

## Utasítás blokkok és vezérlési szerkezetek

Utasítás blokk:

```sql
BEGIN
  DECLARE @darab int
  SET @darab = 3
END
```

Elágazás (ha létezik a felhasználó, frissítsük az email címét):

```sql
DECLARE @nev nvarchar(max)

SELECT @nev = Nev
FROM Vevo
WHERE ID = 123

IF @nev IS NOT NULL
BEGIN
  PRINT 'Email cim frissitese'
  UPDATE Vevo
  SET Email = 'agh*******@gmail.com'
  WHERE ID = 123
END
ELSE
BEGIN
  PRINT 'Nincs ilyen vevo'
END
```

Ciklus (generáljunk legalább 1000 terméket - pl. teszteléshez):

```sql
WHILE (SELECT COUNT(*) FROM Termek) < 1000
BEGIN
    INSERT INTO Termek(Nev,NettoAr,Raktarkeszlet,AFAID,KategoriaID)
    VALUES ('Alma', 1, 1, 3, 13)
END
```

## Tárolt eljárás

ÁFA kulcs rögzítése az ÁFA táblába, olyan kulcs nem rögzíthető mely már létezik:

```sql
create or alter procedure AFARogzites -- tárolt eljárás létrehozása, neve
    @Kulcs int                        -- tárolt eljárás paraméterei
as

begin tran                            -- nem megismételhető olvasás elkerülése végett
set transaction isolation level repeatable read

declare @DB int

select @DB = count(*)
from AFA
where Kulcs = @Kulcs

if @DB = 0
    insert into AFA values (@Kulcs)
else
    print ‘hiba’;

commit
```

Tárolt eljárás meghívása:

```sql
exec AFARogzites 27
```

Tárolt eljárás törlése:

```sql
drop procedure AFARogzites
```

SQL Server 2016 előtt nem volt `create or alter`, csak külön-külön.

## Tárolt függvény

Áfakulcsok lekérése egy adott százalék felett:

```sql
CREATE FUNCTION Afakulcsok(@minkulcs int)
RETURNS TABLE
AS RETURN
(
    SELECT ID, Kulcs FROM AFA
    WHERE Kulcs > @minkulcs
);
```

Használata:

```sql
SELECT *
FROM Afakulcsok(20)
```

## Kivételkezelés

ÁFA kulcs rögzítése az ÁFA táblába, ha már létezik az ÁFA kulcs, dobjon hibát:

```sql
create or alter procedure AFARogzites
    @Kulcs int
as

begin tran
set transaction isolation level repeatable read

declare @DB int

select @DB = count(*)
from AFA
where Kulcs = @Kulcs

if @DB = 0
    insert into AFA values (@Kulcs)
else
    throw 51000, ‘hiba’, 1;

commit
```

Hiba lekezelése (elkapása):

```sql
begin try
  exec AFARogzites 27
end try
begin catch
  SELECT  
    ERROR_NUMBER() AS ErrorNumber,
    ERROR_SEVERITY() AS ErrorSeverity,
    ERROR_STATE() AS ErrorState,
    ERROR_PROCEDURE() AS ErrorProcedure,
    ERROR_LINE() AS ErrorLine,
    ERROR_MESSAGE() AS ErrorMessage;
end catch
```

## Trigger

Naplózzuk a termékek törlését egy napló táblába:

```sql
-- Napló ábla létrehozása
create table Naplo([Leiras] [nvarchar](max) NULL)
go

-- Naplózó trigger
create or alter trigger TermekTorlesNaplo
  on Termek
  for delete
as
insert into Naplo(Leiras)
select 'Termek torolve: ' + convert(nvarchar, d.Nev) from deleted d
```

Tegyük fel, hogy a vevőknek két email címe is van: egy a bejelentkezéshez, és megadhatnak egy másikat, amit az értesítésekhez használni akarnak. Hogy ne kelljen mindig mindkét email címet lekérdezni, és választani a kettő közül, legyen elérhető adatbázisban a valóban használt email cím:

```sql
-- Plusz email cím oszlopok a vevőknek
alter table Vevo
add [ErtesitesiEmail] nvarchar(max), [HasznaltEmail] nvarchar(max)
go

-- Használt email címet frissítő trigger
create or alter trigger VevoHirlevelEmail
  on vevo
  for insert, update
as
update Vevo
set HasznaltEmail = ISNULL(i.ErtesitesiEmail, i.Email)
from Vevo v join inserted i on v.ID = i.ID
```

A megrendelés táblába felvett végösszeg oszlopot (amely a megrendelés teljes nettó ára) automatikusan tartsuk karban:

```sql
create or alter trigger MegrendelesVegosszegKarbatartas
  on MegrendelesTetel
  for insert, update, delete
as

update Megrendeles
set vegosszeg = isnull(vegosszeg,0) + OsszegValtozas
from Megrendeles inner join
        (select i.MegrendelesID, sum(Mennyiseg*NettoAr) as OsszegValtozas
        from inserted i
        group by i.MegrendelesID) MegrValtozas
    on Megrendeles.ID = MegrValtozas.MegrendelesID

update Megrendeles
set vegosszeg = isnull(vegosszeg,0) – OsszegValtozas
from Megrendeles inner join
        (select d.MegrendelesID, sum(Mennyiseg*NettoAr) as OsszegValtozas
        from deleted d
        group by d.MegrendelesID) MegrValtozas
    on Megrendeles.ID = MegrValtozas.MegrendelesID
```

## Kurzor

Keressük meg azon termékeket, amiből alig van raktáron, és ha a legutolsó eladás több, mint egy éve volt, akkor adjunk kedvezményt a termékre:

```sql
DECLARE @TermekNev nvarchar(max)
DECLARE @TermekID int
DECLARE @UtolsoRendeles datetime
DECLARE @NettoAr float

DECLARE Termekek CURSOR FAST_FORWARD READ_ONLY
FOR
  SELECT Id, Nev, NettoAr FROM Termek
  WHERE Raktarkeszlet < 3

OPEN Termekek
FETCH FROM Termekek INTO @TermekId, @TermekNev, @NettoAr
WHILE @@FETCH_STATUS = 0
BEGIN

  SELECT @UtolsoRendeles = MAX(Megrendeles.Datum)
  FROM Megrendeles JOIN MegrendelesTetel ON Megrendeles.Id = MegrendelesTetel.MegrendelesId
  WHERE MegrendelesTetel.TermekID = @TermekId

  PRINT 'Termek ' + convert(nvarchar, @TermekID) + ' utolsó megrendelése '
    + convert(nvarchar, @UtolsoRendeles)

  IF @UtolsoRendeles IS NULL OR @UtolsoRendeles < DATEADD(year, -1, GETDATE())
  BEGIN
    UPDATE Termek
    SET NettoAr = @NettoAr * 0.75
    WHERE Id = @TermekId
  END

  FETCH FROM Termekek INTO @TermekId, @TermekNev, @NettoAr
END
CLOSE Termekek
DEALLOCATE Termekek
```
