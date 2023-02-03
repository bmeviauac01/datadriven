db.customers.createIndex({ "sites._id" : 1 }, { "unique" : true, "name" : "ix_site_id" })

db.customers.insertMany([{
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d92"),
    "name" : "Cody Shelton",
    "bankAccount" : "16489665-05899845-10000038",
    "login" : "cshelton",
    "password" : "huti9haj1s",
    "email" : "cshelton@freemail.hu",
    "mainSiteID" : ObjectId("5d7e4370cffa8e1030fd2d93"),
    "sites" : [{
        "_id" : ObjectId("5d7e4370cffa8e1030fd2d93"),
        "zipCode" : "1051",
        "city" : "Budapest",
        "street" : "Andrássy út 22.",
        "tel" : "061-457-11-03",
        "fax" : "061-457-11-04"
      }]
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d94"),
    "name" : "Erika Mckenzie",
    "bankAccount" : "54255831-15615432-25015126",
    "login" : "erikkka",
    "password" : "gandalf67j",
    "email" : "erikkka@hotmail.com",
    "mainSiteID" : ObjectId("5d7e4370cffa8e1030fd2d95"),
    "sites" : [{
        "_id" : ObjectId("5d7e4370cffa8e1030fd2d95"),
        "zipCode" : "1114",
        "city" : "Budapest",
        "street" : "Bud Spencer street 16.",
        "tel" : "061-569-23-99"
      }, {
        "_id" : ObjectId("5d7e4370cffa8e1030fd2d96"),
        "zipCode" : "3000",
        "city" : "Hatvan",
        "street" : "Vörösmarty tér. 5.",
        "tel" : "0646-319-169",
        "fax" : "0646-319-168"
      }]
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d97"),
    "name" : "Krista Hansen",
    "bankAccount" : "25894467-12005362-59815126",
    "login" : "kris",
    "password" : "jag7guFs",
    "email" : "kris.hansen@gmail.com",
    "mainSiteID" : ObjectId("5d7e4370cffa8e1030fd2d98"),
    "sites" : [{
        "_id" : ObjectId("5d7e4370cffa8e1030fd2d98"),
        "zipCode" : "2045",
        "city" : "Törökbálint",
        "street" : "Main street 17.",
        "tel" : "0623-200-156",
        "fax" : "0623-200-155"
      }]
  }])

db.categories.insertMany([{
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d99"),
    "name" : "Toy"
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d9a"),
    "name" : "Play house"
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d9b"),
    "name" : "Baby toy",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d99")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d9c"),
    "name" : "Construction toy",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d99")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d9d"),
    "name" : "Wooden toy",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d99")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d9e"),
    "name" : "Plush figure",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d99")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2d9f"),
    "name" : "Bicycles",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d99")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da0"),
    "name" : "Months 0-6",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9b")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da1"),
    "name" : "Months 6-18",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9b")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da2"),
    "name" : "Months 18-24",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9b")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da3"),
    "name" : "DUPLO",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9c")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da4"),
    "name" : "LEGO",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9c")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da5"),
    "name" : "Building items",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9c")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da6"),
    "name" : "Building blocks",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9d")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da7"),
    "name" : "Toys for skill development",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9d")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da8"),
    "name" : "Logic toys",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9d")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2da9"),
    "name" : "Craftwork toys",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9d")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2daa"),
    "name" : "Baby taxis",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9f")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dab"),
    "name" : "Motors",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9f")
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dac"),
    "name" : "Tricycle",
    "parentCategoryID" : ObjectId("5d7e4370cffa8e1030fd2d9f")
  }])

db.products.insertMany([{
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dad"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da0"),
    "name" : "Activity playgim",
    "price" : 7488.0,
    "stock" : 21,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    },
    "description" : {
      "product" : {
        "product_size" : {
          "unit" : "cm",
          "width" : 150,
          "height" : 50,
          "depth" : 150
        },
        "package_parameters" : {
          "number_of_packages" : 1,
          "package_size" : {
            "unit" : "cm",
            "width" : 150,
            "height" : 20,
            "depth" : 20
          }
        },
        "description" : "Requires battery (not part of the package).",
        "recommended_age" : "0-18 m"
      }
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dae"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da0"),
    "name" : "Colorful baby book",
    "price" : 1738.0,
    "stock" : 58,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    },
    "description" : {
      "product" : {
        "product_size" : {
          "unit" : "cm",
          "width" : 15,
          "height" : 2,
          "depth" : 15
        },
        "package_parameters" : {
          "number_of_packages" : 1,
          "package_size" : {
            "unit" : "cm",
            "width" : 15,
            "height" : 2,
            "depth" : 15
          }
        },
        "description" : "Round ball with nice colors.",
        "recommended_age" : "0-18 m"
      }
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2daf"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da1"),
    "name" : "Baby telephone",
    "price" : 3725.0,
    "stock" : 18,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    },
    "description" : {
      "product" : {
        "product_size" : {
          "unit" : "cm",
          "width" : 20,
          "height" : 12,
          "depth" : 35
        },
        "package_parameters" : {
          "number_of_packages" : 1,
          "package_size" : {
            "unit" : "cm",
            "width" : 40,
            "height" : 25,
            "depth" : 50
          }
        },
        "description" : "Music is good for the ears. Enjoy.",
        "recommended_age" : "9-36 m"
      }
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db0"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da2"),
    "name" : "Fisher Price hammer toy",
    "price" : 8356.0,
    "stock" : 58,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db1"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da5"),
    "name" : "Mega Bloks 24 pcs",
    "price" : 4325.0,
    "stock" : 47,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db2"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da5"),
    "name" : "Maxi Blocks 56 pcs",
    "price" : 1854.0,
    "stock" : 36,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db3"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da5"),
    "name" : "Building Blocks 80 pcs",
    "price" : 4362.0,
    "stock" : 25,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db4"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da4"),
    "name" : "Lego City harbour",
    "price" : 27563.0,
    "stock" : 12,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    },
    "description" : {
      "product" : {
        "package_parameters" : {
          "number_of_packages" : 1,
          "package_size" : {
            "unit" : "cm",
            "width" : 80,
            "height" : 20,
            "depth" : 40
          }
        },
        "description" : "Number of elements: 695.",
        "recommended_age" : "5-12 y"
      }
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db5"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2da3"),
    "name" : "Lego Duplo Excavator",
    "price" : 6399.0,
    "stock" : 26,
    "vat" : {
      "name" : "Standard Rate",
      "percentage" : 27
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db6"),
    "categoryID" : ObjectId("5d7e4370cffa8e1030fd2d9a"),
    "name" : "Child supervision for 1 hour",
    "price" : 800.0,
    "stock" : 0,
    "vat" : {
      "name" : "Reduced Rate",
      "percentage" : 15
    }
  }])

db.invoiceissuer.insertMany([{
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db7"),
    "name" : "ToysRus",
    "zipCode" : "1119",
    "city" : "Budapest",
    "street" : "Main street 23",
    "taxIdentifier" : "15684995-2-32",
    "bankAccount" : "259476332-15689799-10020065"
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db8"),
    "name" : "BabiesRus",
    "zipCode" : "1119",
    "city" : "Budapest",
    "street" : "Main street 23",
    "taxIdentifier" : "68797867-1-32",
    "bankAccount" : "259476332-15689799-10020065"
  }])

db.orders.insertMany([{
    "_id" : ObjectId("5d7e4370cffa8e1030fd2db9"),
    "customerID" : ObjectId("5d7e4370cffa8e1030fd2d94"),
    "siteID" : ObjectId("5d7e4370cffa8e1030fd2d96"),
    "date" : ISODate("2020-01-17T23:00:00Z"),
    "deadline" : ISODate("2020-01-29T23:00:00Z"),
    "status" : "Delivered",
    "paymentMethod" : {
      "method" : "Cash",
      "deadline" : 0
    },
    "orderItems" : [{
        "amount" : 2,
        "price" : 8356.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2db0"),
        "status" : "Delivered",
        "invoiceItem" : {
          "name" : "Fisher Price hammer",
          "amount" : 2,
          "price" : 8356.0,
          "vatPercentage" : 27
        }
      }, {
        "amount" : 1,
        "price" : 1854.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2db2"),
        "status" : "Delivered",
        "invoiceItem" : {
          "name" : "Maxi Blocks 56 pcs",
          "amount" : 1,
          "price" : 1854.0,
          "vatPercentage" : 27
        }
      }, {
        "amount" : 5,
        "price" : 1738.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2dae"),
        "status" : "Delivered",
        "invoiceItem" : {
          "name" : "Colorful baby book",
          "amount" : 5,
          "price" : 1738.0,
          "vatPercentage" : 27
        }
      }],
    "invoice" : {
      "customerName" : "Erika Mckenzie",
      "customerZipCode" : "3000",
      "customerCity" : "Hatvan",
      "customerStreet" : "Vörösmarty tér. 5.",
      "printedCopies" : 2,
      "cancelled" : false,
      "paymentMethod" : "Cash",
      "creationDate" : ISODate("2020-01-29T23:00:00Z"),
      "deliveryDate" : ISODate("2020-01-29T23:00:00Z"),
      "paymentDeadline" : ISODate("2020-01-29T23:00:00Z"),
      "invoiceIssuer" : {
        "_id" : ObjectId("5d7e4370cffa8e1030fd2db7"),
        "name" : "ToysRus",
        "zipCode" : "1119",
        "city" : "Budapest",
        "street" : "Main street 23",
        "taxIdentifier" : "15684995-2-32",
        "bankAccount" : "259476332-15689799-10020065"
      }
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dba"),
    "customerID" : ObjectId("5d7e4370cffa8e1030fd2d92"),
    "siteID" : ObjectId("5d7e4370cffa8e1030fd2d93"),
    "date" : ISODate("2020-02-12T23:00:00Z"),
    "deadline" : ISODate("2020-02-14T23:00:00Z"),
    "status" : "Delivered",
    "paymentMethod" : {
      "method" : "Wire transfer 8",
      "deadline" : 8
    },
    "orderItems" : [{
        "amount" : 2,
        "price" : 7488.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2dad"),
        "status" : "Delivered",
        "invoiceItem" : {
          "name" : "Activity playgim",
          "amount" : 2,
          "price" : 7488.0,
          "vatPercentage" : 27
        }
      }, {
        "amount" : 3,
        "price" : 3725.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2daf"),
        "status" : "Delivered",
        "invoiceItem" : {
          "name" : "Baby telephone",
          "amount" : 3,
          "price" : 3725.0,
          "vatPercentage" : 27
        }
      }],
    "invoice" : {
      "customerName" : "Cody Shelton",
      "customerZipCode" : "1051",
      "customerCity" : "Budapest",
      "customerStreet" : "Hercegprímás u. 22.",
      "printedCopies" : 2,
      "cancelled" : false,
      "paymentMethod" : "Wire transfer 8",
      "creationDate" : ISODate("2020-02-13T23:00:00Z"),
      "deliveryDate" : ISODate("2020-02-14T23:00:00Z"),
      "paymentDeadline" : ISODate("2020-02-22T23:00:00Z"),
      "invoiceIssuer" : {
        "_id" : ObjectId("5d7e4370cffa8e1030fd2db7"),
        "name" : "ToysRus",
        "zipCode" : "1119",
        "city" : "Budapest",
        "street" : "Main street 23",
        "taxIdentifier" : "15684995-2-32",
        "bankAccount" : "259476332-15689799-10020065"
      }
    }
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dbb"),
    "customerID" : ObjectId("5d7e4370cffa8e1030fd2d94"),
    "siteID" : ObjectId("5d7e4370cffa8e1030fd2d95"),
    "date" : ISODate("2020-02-14T23:00:00Z"),
    "deadline" : ISODate("2020-02-19T23:00:00Z"),
    "status" : "Processing",
    "paymentMethod" : {
      "method" : "Cash",
      "deadline" : 0
    },
    "orderItems" : [{
        "amount" : 1,
        "price" : 4362.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2db3"),
        "status" : "Packaged"
      }, {
        "amount" : 6,
        "price" : 1854.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2db2"),
        "status" : "Processing"
      }, {
        "amount" : 2,
        "price" : 6399.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2db5"),
        "status" : "Packaged"
      }, {
        "amount" : 5,
        "price" : 1738.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2dae"),
        "status" : "New"
      }]
  }, {
    "_id" : ObjectId("5d7e4370cffa8e1030fd2dbc"),
    "customerID" : ObjectId("5d7e4370cffa8e1030fd2d92"),
    "siteID" : ObjectId("5d7e4370cffa8e1030fd2d93"),
    "date" : ISODate("2020-02-14T23:00:00Z"),
    "deadline" : ISODate("2020-02-19T23:00:00Z"),
    "status" : "Packaged",
    "paymentMethod" : {
      "method" : "Credit card",
      "deadline" : 0
    },
    "orderItems" : [{
        "amount" : 23,
        "price" : 3725.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2daf"),
        "status" : "Packaged"
      }, {
        "amount" : 12,
        "price" : 1738.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2dae"),
        "status" : "Packaged"
      }, {
        "amount" : 10,
        "price" : 27563.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2db4"),
        "status" : "Packaged"
      }, {
        "amount" : 25,
        "price" : 7488.0,
        "productID" : ObjectId("5d7e4370cffa8e1030fd2dad"),
        "status" : "Packaged"
      }]
  }])
