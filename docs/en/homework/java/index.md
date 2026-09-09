# 4. Java Persistence API, Spring

By completing the homework assignment, you can earn **4 points**.

Using the department's AHK system, create a repository for yourself. You can find the **invitation URL in Moodle**. Clone the repository you created. It will contain the expected structure of the solution. Create a branch named `solution` and **work on that branch**. After completing the tasks, commit and push your solution.

## Overview of the Starting Code

Import the Maven-based project found in the repository into any Java IDE. The project requires JDK 25. The task is to extend the data access and business logic layers of a fictional logistics application. The logistics company creates transport plans (`TransportPlan`), which summarize how a particular shipment is planned to be delivered through different sections (`Section`), passing through various milestones (`Milestone`). A brief description of the entities in the data model:

* **TransportPlan**: represents a transport plan and contains sections (`Section`). It has a unique identifier and stores the IDs of the related orders.

* **Section**: represents one section of a shipment. It has a starting and an ending milestone (`fromMilestone`, `toMilestone`). The `number` field indicates its position in the transport plan. (Numbering starts from 0.)

* **Milestone**: represents a milestone during transportation. It refers to an address (`Address`) and contains the time at which the milestone is planned to be reached (`plannedTime`). The timestamp is always in local time; the time zone does not need to be stored.

* **Address**: stores address information (two-letter ISO country code, city, street, postal code, house number, latitude, and longitude in degrees).

The starting project also contains tests. An important characteristic of the tests is that you do not need to configure a database at all, because they use an embedded, in-memory H2 database. Therefore, the tasks can be implemented and tested without a persistent database. If you nevertheless want to run the application (`LogisticsApplication`) independently of the test cases and inspect the contents of a persistent database behind it, you need to configure the database connection in `application.properties` and add the JDBC driver dependency to `pom.xml`. The starting project already contains the MSSQL driver dependency, and `application.properties` also contains an example MSSQL JDBC URL. However, this will have no effect on the tests; they will use the in-memory H2 database in all cases.

## Task 0: Neptun Code

As the first step, enter your Neptun code into the `neptun.txt` file located in the project root.

## Task 1: New Queries (2 points)

Extend the `AddressRepository` interface with the following methods:

a. The method should allow querying addresses located in a given city. The comparison should be case-insensitive for the city name, but otherwise an exact match is required.

b. The method should return addresses located within a given "rectangle", specified by the latitude/longitude coordinates of the rectangle's top-left and bottom-right corners.

c. The method should be able to handle street name changes: for addresses that exactly match the country code/postal code/street name triple provided as parameters, change the street name to the new name, which is also provided as a parameter.

To test the queries, run the test methods in the `F1_AddressRepositoryIT` (IT = Integration Test) class. Before using the test class, you must fill in the TODOs in its methods by calling the repository methods you implemented with the appropriate parameters. However, do not modify anything else in the test class.

!!! example "SUBMISSION"
Upload the modified source code.

## Task 2: Extending the Business Logic Layer (2 points)

Implement the following methods of the **`TransportPlanService`** class.

a. **`getFirstAndLastMilestone`**: The method should return a two-element `Milestone` list containing the first and last milestone of the transport plan with the given ID. If the plan does not contain any sections yet, return an empty list. If no plan exists with the given ID, throw an `IllegalArgumentException`.

b. **`registerDelay`**: The method registers an expected delay, specified in minutes, at a given milestone of a given transport plan.

* If the transport plan or milestone does not exist, an `IllegalArgumentException` must be thrown.

* Increase the planned time of the given milestone by the duration of the delay.

* If the milestone is the starting milestone of a section, also increase the planned time of that section's ending milestone by the duration of the delay.

* If the milestone is the ending milestone of a section, increase the planned time of the starting milestone of the next section by the duration of the delay.

Test cases for both methods are provided in the `F2a_TransportPlanServiceGetFirstAndLastMilestoneIT` and `F2b_TransportPlanServiceRegisterDelayIT` classes. You must not modify these classes.

## Task 3 optional: Adding a New Section (0 points)

Implement the **`addSection`** method of `TransportPlanService`. This method allows inserting a new section between the sections of an existing transport plan, with a given section number (`number`) and between two milestones with specified IDs. The following rules must be followed:

* If the transport plan does not exist, or either of the milestones does not exist, an exception must be thrown (`IllegalArgumentException`).

* The number of the new section may be between 0 and MAX, inclusive, where MAX is the number of sections belonging to the plan before the insertion. If this condition is not met, an `IllegalArgumentException` must be thrown.

* The new section may be inserted before, after, or between existing sections. In all cases, the section numbers must remain consecutive. For example, if the existing sections are numbered 0, 1, and 2, and a section with number 1 is inserted, the previous sections numbered 1 and 2 must be renumbered to 2 and 3, respectively. (The subsequent sections must be shifted.)

* The addresses of the milestones of the section immediately preceding or following the new section (if such sections exist) must be checked: the distance between the address of the preceding section's ending milestone and the address of the new section's starting milestone must be within 500 meters. Similarly, the distance between the address belonging to the next section's starting milestone and the address belonging to the new section's ending milestone must also be at most 500 meters. Calculate the distance from the latitude/longitude coordinates using the Haversine formula.

* The planned time of the new section's ending milestone may not be earlier than the planned time of its starting milestone.

* If there is already an existing section before the new section, the planned time of the preceding section's ending milestone may not be later than the planned time of the new section's starting milestone. Otherwise, an `IllegalArgumentException` must be thrown.

* If there is already an existing section after the new section, the planned time of the following section's starting milestone may not be earlier than the planned time of the new section's ending milestone. Otherwise, an `IllegalArgumentException` must be thrown.

* If all of the above conditions are satisfied, a new section must be created in all cases, even if another section already exists between the specified milestones.

Tests for the method are provided in the `F3_TransportPlanServiceAddSectionIT` class. You must not modify this class.

!!! example "SUBMISSION"
Upload the modified source code.
