# JPA & Spring Data

To goal of this seminar is to practice working with JPA and Spring Data. Main topics of focus: working with entities, querying the database with various techniques, updating the database. The code is integrated into a skeleton web application with a UI for testing.

## Pre-requisites

Required tools to complete the tasks:

- Spring Tool Suite (an IDE based on Eclipse)
- Microsoft SQL Server Express edition (localdb does **not** work here)
- SQL Server Management Studio
- Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)
- Starter code: <https://github.com/bmeviauac01/gyakorlat-jpa-kiindulo>

Recommended to review:

- JPA lecture
- EJB, Spring lecture

## How to work during the seminar

The exercises are solved together with the instructor. A few exercises we can try to solve by ourselves and then discuss the results. The final exercise is individual work if time permits.

!!! info ""
    This guide summarizes and explains the behavior. Before looking at these provided answers, we should think first!

## Tips for using the IDE

- Search Type(class, interface, enum): Ctrl+Shift+T (instead of opening folders in Project explorer)
- Search file: Ctrl+Shift+R
- Fix missing imports:Ctrl+Shift+O
- Format code: Ctrl+Shift+F
- In Java Resources right-click a package / New Class/Interfaces will create the source in this package
- Restore default layout of views: Window > Reset perspective
- Increase font size:
    - Window menu / Preferences, start typing _font_ to locate _Fonts and Colors_
    - Select it and under _Basic_ choose _Text Font_ and increase the size

## Exercise 0: Create a database

1. Use _Microsoft SQL Server Management Studio_ to connect to the database. We are not using _localdb_ here; the address is: `localhost\sqlexpress` and use _SQL Server Authentication_ with username and password `sa`.

1. Create a new database with the name `adatvez`. **You should use this exact name or will have to update the Java project**. To create a new database see the instructions [in the first seminar material](../transactions/index.md). If a database with this name already exists, no need to re-create it.

1. Run the database initialization script on this database. If the database exists on this machine, run the script anyway to reset any changes made in the schema.

## Exercise 1: Start the IDE

1. Start Spring Tool Suite from here: `c:\Tools\hatteralkalmazasok\eclipse\SpringToolSuite4.exe`.
1. It will ask for a workspace, select: `c:\Tools\hatteralkalmazasok\workspaces\adatvez`
1. If there is a **webshop** project in the Project Explorer already, delete it: right-click the project / _Delete_, and check _Delete project contents on disk_

## Exercise 2: Import project

1. Download the project skeleton!
    - Open a new _command prompt_
    - Navigate to a directory, e.g. `c:\work\NEPTUN`
    - Execute `git clone --depth 1 https://github.com/bmeviauac01/gyakorlat-jpa-kiindulo.git`
1. Import the downloaded project into the workspace:
    - Open _File / Import..._
    - Start typing _Existing Maven Projects_ and choose it
    - Locate the downloaded webshop project (the `webshop` folder in the checked-out repository), OK, check the webshop project in the dialog
    - Finish
1. Overview of the projects
    - It is a _maven_ based project. Maven is a command-line build tool that can be integrated with IDEs as well. It can download the libraries our projects depend on from public repositories. After opening the `pom.xml` file, the maven project's config file, you can see some dependency tags that will transitively download Hibernate, our JPA implementation, Spring Boot, Spring Data, Spring MVC and Thymeleaf.
    - The application.properties file contains some basic settings. **Let us verify the database name (spring.datasource.url), the user name (spring.datasource.username) and password (spring.datasource.password) for the DB access here.** In classic Java EE web applications, this JNDI name of the database should be defined in the `persistence.xml`, but Spring Boot supports XML-less configuration.
    - `WebshopApplication` is the entry point and configuration of the Spring Boot application. A traditional web application should be deployed to a web container (e.g., Tomcat, Jetty) running in a separate process. In the case of Spring Boot, however, Spring Boot itself will start an embedded web container (Tomcat, by default).
    - The web interface is one page: `src\main\resources\templates\testPage.html`. We will not modify it. It contains standard HTML and some Thymeleaf attributes.
    - `WebshopController`: the controller class implementing the web layer (its methods handle the HTTP requests). These methods typically call a query implemented in a repository or a service method and put the result into the model with a name that we can reference via Thymeleaf. You should call the methods implementing the tasks at the `//TODO` comments.

## Exercise 3: Overview of the entities

- The  entities can be found in the `hu.bme.aut.adatvez.webshop.model` package. We could have written them by hand, but in this case, they were generated from the DB tables via the JPA plugin of Eclipse.
- Open an entity class, e.g., Vat, and check the JPA-related code. You can see the `@Entity`, `@Id` annotations, and `@OneToMany` or `@ManyToOne` for defining relationships.

## Exercise 4: Queries

Implement the following queries on the data model. In JPA and Spring Data, you can write queries by different means. In the following tasks, we specify how to write the query so that multiple ways can be demonstrated.

!!! important ""
    It is important to note that each task could be written using any technology and style. The requirements are provided for demonstrating all technologies.

The methods implementing the queries should always be called in the `WebshopController` class, at the corresponding `//TODO` comment, run the application and test the query from a browser at address <http://localhost:9080>.

**a)** List the names and stock of those products of which we have more than 30 pieces in stock! Method: Spring Data repository interface with method name-derived query.

**b)** Write a query that lists those products that were ordered at least twice! Method: JPQL query created with an injected EntityManager in a Spring Data custom repository implementation.

**c)** List the data of the most expensive product! Method: Named query, called from Spring Data repository or with an injected EntityManager.

When running the application, the SQL statements generated by Hibernate can be observed in the Console view of Eclipse, due to this config line in application.properties: `spring.jpa.show-sql=true`

### Running the application

Right-click on the *webshop* project in the Project Explorer > Debug As > Spring Boot App. This starts the application in debug mode, which starts an embedded web container, and the application is available at <http://localhost:9080> from a browser. Having done this once, we can do it more easily: Click on the Debug icon on the toolbar, and you will see the webshop run there.

![Eclipse run](images/eclipse-run.png)

If under the _Debug_ icon you find _webshop run_, the method above is unnecessary.

The running application can be stopped with the red _Terminate_ icon in the Console view. If we run the application twice, without terminating the first run, the second run will report a port collision on the port 9080 and stop. This second execution will be visible in the _Console_ view, and the _Terminate_ command will be inactive, as this copy has been terminated already. Click on the gray double C icon next to _Terminate_ to close this view, and only the active running process will be visible.

If we close the _Console_ view by mistake, use shortcut _Alt+Shift+Q, C_ or menu _Window / Show View / Console_ to reopen.

We can re-run the application using F11 as well. The workspace is configured to automatically terminate the running instance first in this case, so pressing the Terminate button manually is not needed.

When running the application in debug mode, the modifications in HTML files and some Java ode modifications are immediately actualized, so we only have to refresh the browser to see the effect of the code modification. But the application has to be restarted if we modify the Java code in either of the following ways

- adding a new type
- adding/removing/modifying an annotation
- adding a new class-or member variable, or method
- we changed the signature of a method.

Simply put, when modifying code that is not inside of an existing method, a restart will be needed.

??? example "Solution"
    **4.a exercise**

    In the `dao` package open `ProductRepository` interface that implements the Spring Data `JpaRepository`. There are a few methods for other exercises. Some define a `@Query` annotation and the query as text, some work without such annotation. We will not need the `@Query` annotation, but rather have Spring Data infer the SQL instruction from the method name as follows:
    
    ```java
    package hu.bme.aut.adatvez.webshop.dao;
    
    import java.math.BigDecimal;
    import java.util.List;
    import hu.bme.aut.adatvez.webshop.model.Product;
    import org.springframework.data.jpa.repository.JpaRepository;
    
    public interface ProductRepository extends JpaRepository<Product, Long>, ProductRepositoryCustom {
      ...
      List<Product> findByStockGreaterThan(BigDecimal limit);
    }
    ```
    
    `WebshopController` already contains an injected `ProductRepository`; let us call this method at TODO 4.a:
    
    ```java
    @Controller
    public class WebshopController {
    
      @Autowired
      ProductRepository productRepository;
    
      //...
      // 4.a
      private List<Product> findProductsOver30() {
        return productRepository.findByStockGreaterThan(BigDecimal.valueOf(30));
      }
    }
    ```
    
    **4.b exercise**
    
    In the `dao` package find `ProductRepositoryCustom` interface add a new method `findProductsOrderedAtLeastTwice`:
    
    ```java
    package hu.bme.aut.adatvez.webshop.dao;
    
    import hu.bme.aut.adatvez.webshop.model.Product;
    import java.util.List;
    
    public interface ProductRepositoryCustom {
      List<Product> findProductsOrderedAtLeastTwice();
    }
    ```
    
    The implementation class `ProductRepositoryImpl` will contain an error now as it does not implement `ProductRepositoryCustom`. Let us open this class, and line of the class declaration there will be a light bulb we can click to generate the method skeleton:
    
    ![Eclise implement interface](images/eclipse-implement-methods.png)
    
    Add the method's implementation as follows: use the injected EntityManager to create and run the query.
    
    ```java
    package hu.bme.aut.adatvez.webshop.dao;
    
    import hu.bme.aut.adatvez.webshop.model.Product;
    
    import java.util.List;
    
    import jakarta.persistence.EntityManager;
    import jakarta.persistence.PersistenceContext;
    
    public class ProductRepositoryImpl implements ProductRepositoryCustom {
    
      @PersistenceContext
      EntityManager em;
    
      @Override
      public List<Product> findProductsOrderedAtLeastTwice(){
        return em.createQuery("SELECT DISTINCT p FROM Product p
                              LEFT JOIN FETCH p.orderitems
                              WHERE size(p.orderitems) >= :itemsMin", Product.class)
              .setParameter("itemsMin", 2)
              .getResultList();
      }
    }
    ```
    
    Note: we might try this command: `SELECT p FROM Product p WHERE size(p.orderitems) /= :itemsMin`, which will yield an `org.hibernate.LazyInitializationException` error, hence the `LEFT JOIN FETCH` above.
    
    Call this in `WebshopController`:
    
    ```java
    // 4.b
    private List<Product> findProductsOrderedAtLeastTwice() {
      // TODO
      return productRepository.findProductsOrderedAtLeastTwice();
    }
    ```
    
    **4.c exercise**
    
    Open entity class `Product` where we can find a few named querys; we need the second one:
    
    ```java
    @NamedQueries({
    @NamedQuery(name="Product.findAll", query="SELECT p FROM Product p"),
    @NamedQuery(name="Product.findMostExpensive", query="SELECT p FROM Product p WHERE p.price IN (SELECT MAX(p2.price) FROM Product p2)")
    })
    ```
    
    This named query can be called in two ways. The first is to create a method in `ProductRepository` with the same name (without the _Product._ prefix.), that is:
    
    ```java
    public List<Product> findMostExpensive();
    ```
    
    The second option is to execute it manually in `ProductRepositoryImpl` using `EntityManager`:
    
    ```java
    @Override
    public List<Product> findMostExpensiveProducts(){
      return em.createNamedQuery("Product.findMostExpensive", Product.class).getResultList();
    }
    ```
    
    This method also needs to be added to the `ProductRepositoryCustom` interface. E.g. right-click / _Refactor / Pull up_
    
    Finally, call the method in `WebshopController`:
    
    ```java
    // 4.c
    private List<Product> findMostExpensiveProducts() {
      // TODO
      // return productRepository.findMostExpensiveProducts();
      return productRepository.findMostExpensive();
    }
    ```

## Exercise 5: Data modification

JPA can also be used to modify the database content.

**a)** Write a JPQL query into the `ProductRepository` interface that raises the price of "Building items" by 10 percent!

**b)** Write a method that creates a new category called "Expensive toys", if it does not exist yet, and move all the products with a price higher than 8000 into this category!

**c)** Simple individual task: create a `CategoryRepository` interface, and implement a method name-derived query that you can use in task 5.b) instead of the query created with the injected EntityManager.

??? example "Solution"
    **5.a exercise**

    Crate an _UPDATE query_ in `ProductRepository` interface. We have to denote that this is a `@Modifying` query, and also add @Transactional` (from package `org.springframework...`):
    
    ```java
    @Modifying
    @Transactional
    @Query("UPDATE Product p SET p.price=p.price*1.1 WHERE p.id IN
    (SELECT p2.id FROM Product p2 WHERE p2.category.name=:categoryName)")
    void categoryRaisePrice(@Param("categoryName") String categoryName);
    ```
    
    Call in `WebshopController`:
    
    ```java
    // 5.a
    @RequestMapping(value = "/raisePriceOfBuildingItems", method = {
            RequestMethod.POST, RequestMethod.GET })
    private String raisePriceOfBuildingItems() {
      // TODO
      productRepository.categoryRaisePrice("Building items");
      return "redirect:/";
    }
    ```
    
    In the browser, the changes are visible after clicking the button.
    
    **5.b exercise**
    
    In the `dao` package add a new class `CategoryService` with a `@Service` annotation with a `@Transactional` method:
    
    ```java
    @Service
    public class CategoryService {
    
      @PersistenceContext
      private EntityManager em;
    
      @Autowired
      ProductRepository productRepository;
    
      @Transactional
      public void moveToExpensiveToys(double priceLimit){
        String name = "Expensive toys";
        Category categoryExpensive = null;
        List<Category> resultList =
          em.createQuery("SELECT c from Category c WHERE c.name=:name", Category.class)
            .setParameter("name", name)
            .getResultList();
    
        if(resultList.isEmpty()){
          // 0 or null id triggers @GeneratedValue; this is a scalar, hence use 0
          categoryExpensive = new Category(0, name);
          em.persist(categoryExpensive);
        }else{
          categoryExpensive = resultList.get(0);
        }
    
        List<Product> expensiveProducts = productRepository.findByPriceGreaterThan(priceLimit);
    
        for (Product product : expensiveProducts) {
          categoryExpensive.addProduct(product);
        }
      }
    }
    ```
    
    Let us note that the managed entities (fetched through queries within the transaction, or added as new with persist) need no explicit save; the transaction saves them to DB automatically.
    
    Call in `WebshopController`:
    
    ```java
    @Autowired
    CategoryService categoryService;
    ...
    
    // 5.b
    @RequestMapping(value = "/moveToExpensiveToys", method = {
            RequestMethod.POST, RequestMethod.GET })
    private String moveToExpensiveToys() {
      // TODO
      categoryService.moveToExpensiveToys(8000.0);
      return "redirect:/";
    }
    ```
    
    In the browser, the changes are visible after clicking the button.
    
    **5.c exercise**
    
    In `dao` package add a new interface `CategoryRepository`, similar to `ProductRepository` (without the Custom inheritance) with one method:
    
    ```java
    public interface CategoryRepository extends JpaRepository<Category, Long>{
      List<Category> findByName(String name);
    }
    ```
    
    This simplifies the `CategoryService` as follows:
    
    ```java
    @Service
    public class CategoryService {
    ...
    
      @Autowired
      CategoryRepository categoryRepository;
    
      @Transactional
      public void moveToExpensiveToys(double priceLimit){
        // ...
        List<Category> resultList = categoryRepository.findByName(name);
        //  ...
      }
    }
    ```

## Exercise 6: Using stored procedures

Use the `CreatePaymentMethod` stored procedure to create a new `Paymentmethod`!

- Check in SQL Server Management Studio, whether the database contains the stored procedure with the name `CreatePaymentMethod`!

- If not, create the procedure with the code below!

    ```sql
    CREATE PROCEDURE CreateNewPaymentMethod
    (
    @Method nvarchar(20),
    @Deadline int
    )
    AS
    insert into PaymentMethod
    values(@Method,@Deadline)
    select scope_identity() as NewId
    ```

??? example "Solution"
    The `PaymentMethod` entity has the following annotation. Compare it to the stored procedure code!

    ```java
    @NamedStoredProcedureQueries({
      @NamedStoredProcedureQuery(name = "createMethodSP",
          procedureName = "CreateNewPaymentMethod",
          parameters = {
                @StoredProcedureParameter(mode = ParameterMode.IN, name = "Method", type = String.class),
                @StoredProcedureParameter(mode = ParameterMode.IN, name = "Deadline", type = BigDecimal.class)
              })
    })
    public class Paymentmethod implements Serializable {
    ...
    ```
    
    The named stored procedure query can be called from a Spring Data repository (`dao` package _New Interface ... / PaymentmethodRepository_):
    
    ```java
    public interface PaymentmethodRepository extends JpaRepository<Paymentmethod, Long> {
    
      @Procedure(name="createMethodSP")
      void newMethod(@Param("Method") String method, @Param("Deadline") BigDecimal deadline);
    }
    ```
    
    Without Spring Data we could use `EntityManager`:
    
    ```java
    @Service
    public class PaymentmethodService {
    
      @PersistenceContext
      private EntityManager em;
    
      public void createNewMethod(Paymentmethod paymentMethod){
        StoredProcedureQuery sp = em.createNamedStoredProcedureQuery("createMethodSP");
        sp.setParameter("Method", paymentMethod.getMethod());
        sp.setParameter("Deadline", paymentMethod.getDeadline());
        sp.execute();
      }
    }
    ```
    
    Call from the web layer:
    
    - Inject into `WebshopController` the `PaymentmethodRepository` interface:
    
        ```java
        @Autowired
        PaymentmethodRepository paymentmethodRepository;
        ```
    
    - Call the method from WebshopController at the last TODO
    
        ```java
        paymentmethodRepository.newMethod(paymentMethod.getMethod(), paymentMethod.getDeadline());
        ```
