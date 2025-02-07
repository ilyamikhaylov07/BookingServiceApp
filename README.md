# Бэкенд для сервиса записи к специалистам

## Описание

Разработка бэкенда для приложения записи к специалистам различного рода деятельности с использованием микросервисной архитектуры.

My first project with a micro service architecture. All services are deployed through Docker, including RabbitMQ. I decided not to deploy the database and used a local client. JWT is used for authorization. Passwords in the database are hashed. RabbitMQ is used with the MassTransit bus to abstract the interaction, since everything is already under the hood, thereby simplifying the configuration of the message broker. If someone is going to study micro service architecture on ASP.NET, I think this project will help you to roughly imagine.

## Основные функции

- Приложение поделено на три микросервиса:
  - **Сервис специалистов**
  - **Сервис записи**
  - **Сервис авторизации**
- Развертывание всех сервисов при помощи **Docker**.
- Использование **RabbitMQ** для взаимодействия между сервисами.
- Применение **MassTransit** для упрощения логики взаимодействия.
- **JWT-аутентификация** с двумя схемами: **access** и **refresh**.
- Хеширование паролей в базе данных, сравнение по хешу при авторизации.
- Реализация основных **CRUD-операций** для каждого сервиса.
- Логирование во всём приложении с помощью **Serilog** (по умолчанию в консоль, но можно настроить иначе).

## Используемые технологии

- **ASP.NET**
- **Entity Framework**
- **PostgreSQL**
- **RabbitMQ**
- **MassTransit**
- **Docker**
- **Serilog**

## Установка и запуск

1. Клонируйте репозиторий:
   ```sh
   git clone https://github.com/ilyamikhaylov07/BookingServiceApp
   cd your-repo
   ```
2. Настройте окружение и убедитесь, что у вас установлен **Docker**.
3. Запустите все сервисы через Docker Compose:
   ```sh
   docker-compose up -d
   ```
4. Приложение готово к использованию!

## Контакты

Если у вас есть вопросы или предложения, свяжитесь с нами по email: `mr.ilmix@mail@.ru`.
