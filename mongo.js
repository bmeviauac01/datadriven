db.vevok.createIndex({ "telephelyek._id" : 1 }, { "unique" : true, "name" : "ix_telephely_id" })

db.vevok.insertMany([{
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbb4"),
    "nev" : "Puskás Norbert",
    "szamlaszam" : "16489665-05899845-10000038",
    "login" : "pnorbert",
    "jelszo" : "huti9haj1s",
    "email" : "puskasnorbert@freemail.hu",
    "kozpontiTelephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbb5"),
    "telephelyek" : [{
        "_id" : ObjectId("5d7e42adcffa8e1b64f7dbb5"),
        "ir" : "1051",
        "varos" : "Budapest",
        "utca" : "Hercegprímás u. 22.",
        "tel" : "061-457-11-03",
        "fax" : "061-457-11-04"
      }]
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbb6"),
    "nev" : "Hajdú-Nagy Katalin",
    "szamlaszam" : "54255831-15615432-25015126",
    "login" : "katinka",
    "jelszo" : "gandalf67j",
    "email" : "hajdunagyk@hotmail.com",
    "kozpontiTelephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbb7"),
    "telephelyek" : [{
        "_id" : ObjectId("5d7e42adcffa8e1b64f7dbb7"),
        "ir" : "1114",
        "varos" : "Budapest",
        "utca" : "Baranyai u. 16.",
        "tel" : "061-569-23-99"
      }, {
        "_id" : ObjectId("5d7e42adcffa8e1b64f7dbb8"),
        "ir" : "3000",
        "varos" : "Hatvan",
        "utca" : "Vörösmarty tér. 5.",
        "tel" : "0646-319-169",
        "fax" : "0646-319-168"
      }]
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbb9"),
    "nev" : "Grosz János",
    "szamlaszam" : "25894467-12005362-59815126",
    "login" : "jano",
    "jelszo" : "jag7guFs",
    "email" : "janos.grosz@gmail.com",
    "kozpontiTelephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbba"),
    "telephelyek" : [{
        "_id" : ObjectId("5d7e42adcffa8e1b64f7dbba"),
        "ir" : "2045",
        "varos" : "Törökbálint",
        "utca" : "Határ u. 17.",
        "tel" : "0623-200-156",
        "fax" : "0623-200-155"
      }]
  }])

db.kategoriak.insertMany([{
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbbb"),
    "nev" : "Játék"
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbbc"),
    "nev" : "Játszóház"
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbbd"),
    "nev" : "Bébijáték",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbb")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbbe"),
    "nev" : "Építojáték",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbb")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbbf"),
    "nev" : "Fajáték",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbb")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc0"),
    "nev" : "Plüss figurák",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbb")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc1"),
    "nev" : "Közlekedési eszközök",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbb")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc2"),
    "nev" : "0-6 hónapos kor",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbd")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc3"),
    "nev" : "6-18 hónapos kor",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbd")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc4"),
    "nev" : "18-24 hónapos kor",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbd")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc5"),
    "nev" : "DUPLO",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbe")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc6"),
    "nev" : "LEGO",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbe")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc7"),
    "nev" : "Építo elemek",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbe")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc8"),
    "nev" : "Építo kockák",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbf")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbc9"),
    "nev" : "Készségfejleszto játékok",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbf")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbca"),
    "nev" : "Logikai játékok",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbf")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbcb"),
    "nev" : "Ügyességi játékok",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbf")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbcc"),
    "nev" : "Bébi taxik",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc1")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbcd"),
    "nev" : "Motorok",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc1")
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbce"),
    "nev" : "Triciklik",
    "szuloKategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc1")
  }])

db.termekek.insertMany([{
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbcf"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc2"),
    "nev" : "Activity playgim",
    "nettoAr" : 7488.0,
    "raktarkeszlet" : 21,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    },
    "leiras" : {
      "termek" : {
        "termek_meret" : {
          "mertekegyseg" : "cm",
          "szelesseg" : 150,
          "magassag" : 50,
          "melyseg" : 150
        },
        "csomag_parameterek" : {
          "csomag_darabszam" : 1,
          "csomag_meret" : {
            "mertekegyseg" : "cm",
            "szelesseg" : 150,
            "magassag" : 20,
            "melyseg" : 20
          }
        },
        "leiras" : "\r\n\t\t\tElemmel mukodik, a csomag nem tartalmay elemet.\r\n\t\t",
        "ajanlott_kor" : "0-18 hónap"
      }
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd0"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc2"),
    "nev" : "Színes bébikönyv",
    "nettoAr" : 1738.0,
    "raktarkeszlet" : 58,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    },
    "leiras" : {
      "termek" : {
        "termek_meret" : {
          "mertekegyseg" : "cm",
          "szelesseg" : 15,
          "magassag" : 2,
          "melyseg" : 15
        },
        "csomag_parameterek" : {
          "csomag_darabszam" : 1,
          "csomag_meret" : {
            "mertekegyseg" : "cm",
            "szelesseg" : 15,
            "magassag" : 2,
            "melyseg" : 15
          }
        },
        "leiras" : "\r\n\t\t\tTiszta pamut oldalak, élénk színek, vastag kontúrok.\r\n\t\t\tEz a mini világ termék a babák életkori sajátosságainak megfeleloen fejleszti a látást, tapintást. Motiválja a babát, hogy megtanulja környezete felismerését.\r\n\t\t\tFelerosítheto a gyerekágyra, járókára vagy a babakocsira.\r\n\t\t",
        "ajanlott_kor" : "0-18 hónap"
      }
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd1"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc3"),
    "nev" : "Zenélo bébitelefon",
    "nettoAr" : 3725.0,
    "raktarkeszlet" : 18,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    },
    "leiras" : {
      "termek" : {
        "termek_meret" : {
          "mertekegyseg" : "cm",
          "szelesseg" : 20,
          "magassag" : 12,
          "melyseg" : 35
        },
        "csomag_parameterek" : {
          "csomag_darabszam" : 1,
          "csomag_meret" : {
            "mertekegyseg" : "cm",
            "szelesseg" : 40,
            "magassag" : 25,
            "melyseg" : 50
          }
        },
        "leiras" : "\r\n\t\t\t9-36 hónaposan a zajok és a zene izgatja a gyermeki fantáziát. A gombok különbözo hangélményekkel lepik meg a gyermeket a dallamok és csengetések segítségével. A 3 gomb megnyomásával vidám képmotívumok kezdenek forogni.\r\n\t\t",
        "ajanlott_kor" : "9-36 hónap"
      }
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd2"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc4"),
    "nev" : "Fisher Price kalapáló",
    "nettoAr" : 8356.0,
    "raktarkeszlet" : 58,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd3"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc7"),
    "nev" : "Mega Bloks 24 db-os",
    "nettoAr" : 4325.0,
    "raktarkeszlet" : 47,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd4"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc7"),
    "nev" : "Maxi Blocks 56 db-os",
    "nettoAr" : 1854.0,
    "raktarkeszlet" : 36,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd5"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc7"),
    "nev" : "Building Blocks 80 db-os",
    "nettoAr" : 4362.0,
    "raktarkeszlet" : 25,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd6"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc6"),
    "nev" : "Lego City kikötoje",
    "nettoAr" : 27563.0,
    "raktarkeszlet" : 12,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    },
    "leiras" : {
      "termek" : {
        "csomag_parameterek" : {
          "csomag_darabszam" : 1,
          "csomag_meret" : {
            "mertekegyseg" : "cm",
            "szelesseg" : 80,
            "magassag" : 20,
            "melyseg" : 40
          }
        },
        "leiras" : "\r\n\t\t\tElemek száma: 695 db.\r\n\t\t",
        "ajanlott_kor" : "5-12 év"
      }
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd7"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbc5"),
    "nev" : "Lego Duplo Ásógép",
    "nettoAr" : 6399.0,
    "raktarkeszlet" : 26,
    "afa" : {
      "nev" : "Általános",
      "kulcs" : 20
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd8"),
    "kategoriaID" : ObjectId("5d7e42adcffa8e1b64f7dbbc"),
    "nev" : "Egy óra gyerekfelügyelet",
    "nettoAr" : 800.0,
    "raktarkeszlet" : 0,
    "afa" : {
      "nev" : "Kedvezményes",
      "kulcs" : 15
    }
  }])

db.szamlakiallito.insertMany([{
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd9"),
    "nev" : "Regio Játék Áruház Kft",
    "ir" : "1119",
    "varos" : "Budapest",
    "utca" : "Nándorfejérvári u. 23",
    "adoszam" : "15684995-2-32",
    "szamlaszam" : "259476332-15689799-10020065"
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbda"),
    "nev" : "Regio Játék Áruház Zrt",
    "ir" : "1119",
    "varos" : "Budapest",
    "utca" : "Nándorfejérvári u. 23",
    "adoszam" : "68797867-1-32",
    "szamlaszam" : "259476332-15689799-10020065"
  }])

db.megrendelesek.insertMany([{
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbdb"),
    "vevoID" : ObjectId("5d7e42adcffa8e1b64f7dbb6"),
    "telephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbb8"),
    "datum" : ISODate("2008-01-17T23:00:00Z"),
    "hatarido" : ISODate("2008-01-29T23:00:00Z"),
    "statusz" : "Kiszállítva",
    "fizetesMod" : {
      "mod" : "Készpénz",
      "hatarido" : 0
    },
    "megrendelesTetelek" : [{
        "mennyiseg" : 2,
        "nettoAr" : 8356.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd2"),
        "statusz" : "Kiszállítva",
        "szamlaTetel" : {
          "nev" : "Fisher Price kalapáló",
          "mennyiseg" : 2,
          "nettoAr" : 8356.0,
          "afaKulcs" : 20
        }
      }, {
        "mennyiseg" : 1,
        "nettoAr" : 1854.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd4"),
        "statusz" : "Kiszállítva",
        "szamlaTetel" : {
          "nev" : "Maxi Blocks 56 db-os",
          "mennyiseg" : 1,
          "nettoAr" : 1854.0,
          "afaKulcs" : 20
        }
      }, {
        "mennyiseg" : 5,
        "nettoAr" : 1738.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd0"),
        "statusz" : "Kiszállítva",
        "szamlaTetel" : {
          "nev" : "Színes bébikönyv",
          "mennyiseg" : 5,
          "nettoAr" : 1738.0,
          "afaKulcs" : 20
        }
      }],
    "szamla" : {
      "megrendeloNev" : "Hajdú-Nagy Katalin",
      "megrendeloIR" : "3000",
      "megrendeloVaros" : "Hatvan",
      "megrendeloUtca" : "Hatvan",
      "nyomtatottPeldanyszam" : 2,
      "sztorno" : false,
      "fizetesiMod" : "Készpénz",
      "kiallitasDatum" : ISODate("2008-01-29T23:00:00Z"),
      "teljesitesDatum" : ISODate("2008-01-29T23:00:00Z"),
      "fizetesiHatarido" : ISODate("2008-01-29T23:00:00Z"),
      "kiallito" : {
        "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd9"),
        "nev" : "Regio Játék Áruház Kft",
        "ir" : "1119",
        "varos" : "Budapest",
        "utca" : "Nándorfejérvári u. 23",
        "adoszam" : "15684995-2-32",
        "szamlaszam" : "259476332-15689799-10020065"
      }
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbdc"),
    "vevoID" : ObjectId("5d7e42adcffa8e1b64f7dbb4"),
    "telephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbb5"),
    "datum" : ISODate("2008-02-12T23:00:00Z"),
    "hatarido" : ISODate("2008-02-14T23:00:00Z"),
    "statusz" : "Kiszállítva",
    "fizetesMod" : {
      "mod" : "Átutalás 8",
      "hatarido" : 8
    },
    "megrendelesTetelek" : [{
        "mennyiseg" : 2,
        "nettoAr" : 7488.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbcf"),
        "statusz" : "Kiszállítva",
        "szamlaTetel" : {
          "nev" : "Activity playgim",
          "mennyiseg" : 2,
          "nettoAr" : 7488.0,
          "afaKulcs" : 20
        }
      }, {
        "mennyiseg" : 3,
        "nettoAr" : 3725.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd1"),
        "statusz" : "Kiszállítva",
        "szamlaTetel" : {
          "nev" : "Zenélo bébitelefon",
          "mennyiseg" : 3,
          "nettoAr" : 3725.0,
          "afaKulcs" : 20
        }
      }],
    "szamla" : {
      "megrendeloNev" : "Puskás Norbert",
      "megrendeloIR" : "1051",
      "megrendeloVaros" : "Budapest",
      "megrendeloUtca" : "Budapest",
      "nyomtatottPeldanyszam" : 2,
      "sztorno" : false,
      "fizetesiMod" : "Átutalás 8",
      "kiallitasDatum" : ISODate("2008-02-13T23:00:00Z"),
      "teljesitesDatum" : ISODate("2008-02-14T23:00:00Z"),
      "fizetesiHatarido" : ISODate("2008-02-22T23:00:00Z"),
      "kiallito" : {
        "_id" : ObjectId("5d7e42adcffa8e1b64f7dbd9"),
        "nev" : "Regio Játék Áruház Kft",
        "ir" : "1119",
        "varos" : "Budapest",
        "utca" : "Nándorfejérvári u. 23",
        "adoszam" : "15684995-2-32",
        "szamlaszam" : "259476332-15689799-10020065"
      }
    }
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbdd"),
    "vevoID" : ObjectId("5d7e42adcffa8e1b64f7dbb6"),
    "telephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbb7"),
    "datum" : ISODate("2008-02-14T23:00:00Z"),
    "hatarido" : ISODate("2008-02-19T23:00:00Z"),
    "statusz" : "Várakozik",
    "fizetesMod" : {
      "mod" : "Készpénz",
      "hatarido" : 0
    },
    "megrendelesTetelek" : [{
        "mennyiseg" : 1,
        "nettoAr" : 4362.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd5"),
        "statusz" : "Csomagolva"
      }, {
        "mennyiseg" : 6,
        "nettoAr" : 1854.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd4"),
        "statusz" : "Várakozik"
      }, {
        "mennyiseg" : 2,
        "nettoAr" : 6399.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd7"),
        "statusz" : "Csomagolva"
      }, {
        "mennyiseg" : 5,
        "nettoAr" : 1738.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd0"),
        "statusz" : "Rögzítve"
      }]
  }, {
    "_id" : ObjectId("5d7e42adcffa8e1b64f7dbde"),
    "vevoID" : ObjectId("5d7e42adcffa8e1b64f7dbb4"),
    "telephelyID" : ObjectId("5d7e42adcffa8e1b64f7dbb5"),
    "datum" : ISODate("2008-02-14T23:00:00Z"),
    "hatarido" : ISODate("2008-02-19T23:00:00Z"),
    "statusz" : "Csomagolva",
    "fizetesMod" : {
      "mod" : "Kártya",
      "hatarido" : 0
    },
    "megrendelesTetelek" : [{
        "mennyiseg" : 23,
        "nettoAr" : 3725.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd1"),
        "statusz" : "Csomagolva"
      }, {
        "mennyiseg" : 12,
        "nettoAr" : 1738.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd0"),
        "statusz" : "Csomagolva"
      }, {
        "mennyiseg" : 10,
        "nettoAr" : 27563.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbd6"),
        "statusz" : "Csomagolva"
      }, {
        "mennyiseg" : 25,
        "nettoAr" : 7488.0,
        "termekID" : ObjectId("5d7e42adcffa8e1b64f7dbcf"),
        "statusz" : "Csomagolva"
      }]
  }])

