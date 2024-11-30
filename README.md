# Project description

This project is showcase project for using the following in ASP.NET Core:
 - MongoDb with EFCore
     - CRUD operations
     - MongodDb docker-compose for running the instance
 - REDIS for generating the shared entity sequence
     - Using INCR built-in functionality 
     - REDIS docker-compose for running the instance
 - RabbitMQ for publishing and consuming events
     - Publishing and consuming messages via MassTransit 
     - Fault tolerance with delayer message retry
     - RabbitMQ docker-compose for running the instance
