# DLeh.Console

This repository contains a helper library to facilitate creating runnable console commands. This allows you to easily create C# code that integrates with your codebase and can be called at any time against configurable environments.


## Sample Use Cases
* Run Entity Framework migrations on Local, Dev, or whatever environment by simply selecting which environment you want it to run on
* Run one-time data fixes that abide by the business logic in your codebase by calling your normal service methods
* Create informational queries for your database (like LinqPad but you can use all your existing code for filtering or populating business information)
* End-to-end testing scripts can be built in a command to test API functions without a fully-built UI. 
* Kick off tasks, call your other microservices, integration test calls to other APIs, etc! The possibilities are [endless!](http://www.zombo.com/)

