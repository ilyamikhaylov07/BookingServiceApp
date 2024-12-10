My first project with a micro service architecture. All services are deployed through docker, including RabbitMQ. I decided not to deploy the database, and used a local client. JWT is used for authorization. Passwords in the database are hashed. RabbitMQ is used with the MassTransit bus to abstract the interaction, since everything is already under the hood, thereby simplifying the configuration of the message broker. If someone is going to study micro service architecture on ASP.NET I think this project will help you to roughly imagine.
