# Getting-Started-with-Entity-Framework-Core-Code-First-Using-ASP.NET-MVC-Core (SAMPLE PROJECT)
## Background
This project is based off the tutorial and project [Getting Started with Entity Framework 6 Code First using MVC 5 by Tom Dykstra and Rick Anderson](http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/).  This project aims to cover as much of the same functionality using ASP.NET Core 1.0 and Entity Framework Core 1.0 as possible.  I have also updated using new features like tag helpers as much as possible.  Also included in this project but not in the original is my attempt to apply the **Repository** and **Unit of Work Patterns**.  

I encourage feedback and please let me know where things can or should be refactored to make them easier to understand or more efficient.  This was a first go round at matching functionality from the original and there is probably ample room for improvment. 

## Additonal Features
* Generic Repository Pattern
* Unit of Work Pattern
* Logging using [Serilog](http://serilog.net/)
* Updates from HTML Helpers to Tag Helpers
## Features not migrated from the orginal tutorial
* The following items were not migrated as they are not currenly supported in EF Core
  * Connection Resiliency
  * SQL Command Interception
  * Lazy Loading
  * Many-to-many relationships without join entity.
    * This example utilizes a join entity to support the many-to-many functionality. Which adds some complexity but the sample code should help you better understand how to use the join entity.
  * Use stored procedures for inserting, updating, and deleting
    * Right now it really looks like you can only call stored procedures pull data. (*This needs to be further verified*)
 ## Notable things I learned
* Tag Helpers not seeing model changes on validation reposts.  This is a good explaination from Rick Strahl on what is going on [ASP.NET MVC Postbacks and HtmlHelper Controls ignoring Model Changes ](https://weblog.west-wind.com/posts/2012/Apr/20/ASPNET-MVC-Postbacks-and-HtmlHelper-Controls-ignoring-Model-Changes)
  * Yo can see how I worked around this issue in the DepartmentController Edit function and the Department Edit view.  Look for the RowVersion and the validation logic in the views.
* Using the Generic Repository and the Unit of Work patterns is a blessing and a curse.
  * I was unable to make a complete separation of needing to reference EF libraries in the controller.  This was due to the lack of Lazy loading and the need to build IQueryable objects for filtering and nested includes of properties.
* Due to the lack of lazy loading and it was neccessary to use the Include and ThenInclude functionality of EF Core quite extensively.  This code sample should give you plenty of examples of how to use that.

