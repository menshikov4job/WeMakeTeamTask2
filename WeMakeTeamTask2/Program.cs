using System.Text.Json;
using System;
using WeMakeTeamTask2;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
{
    var request = context.Request;
    var response = context.Response;

    if (request.Path != "/" || request.IsHttps)
    {
        context.Response.StatusCode = 404;
        return;
    }

    string method = "GET";
    if (context.Request.Method != method)
    {
        await response.WriteAsJsonAsync(
            new { result = 0, errMessage = "Неверный метод запроса!", errCode = 1, note = $"Доступен только {method} метод." });
        return;
    }


    if (request.Query.Count != 1)
    {
        await response.WriteAsJsonAsync(
            new { result = 0, errMessage = $"Неверное кол-во параметров в запросе ({request.Query.Count})!", errCode = 101, 
                note = "Должен быть один параметр." });
        return;
    }

    string param4Insert = "insert";
    string param4Get = "get";

    if (request.Query.ContainsKey(param4Insert))
    {
        await InsertEntity(request.Query[param4Insert], response);
    }
    else if (request.Query.ContainsKey(param4Get))
    {
        await GetEntity(request.Query[param4Get], response);
    }
    else
    {
        await response.WriteAsJsonAsync(
            new { rsult = 0, errMessage = "Отсутсвует нужный параметр в запросе!", errCode = 102, 
                note = "Передайте один из параметров - insert, get" });
    }
});

app.Run();



async Task InsertEntity(string paramValue, HttpResponse response)
{
    try
    {
        var entity = Json.CreateEntity<Entity>(paramValue);
        if (entity != null)
        {
            // Тут какая нибуть проверка полученных данных, проверил были ли поля заполенены из json (переданы в запросе)
            string validateSetFields = entity.ValidateSetFields();
            if (!String.IsNullOrEmpty(validateSetFields))
            {
                await response.WriteAsJsonAsync(
                    new { result = 0, errMessage = $"Не все поля были заполнены: {validateSetFields}", errCode = 201, 
                        note = "Проверьте наличие полей в запросе." });
                return;
            }

            using (var context = new AppDbContext())
            {
                var entityRepository = new EntityRepository(context);
                await entityRepository.InsertAsync(entity);
            }

            //response.StatusCode = 201;
            // успешно
            await response.WriteAsJsonAsync(
                    new { result = 1, note = "Данные добавлены" });
        }
        else
        {
            await response.WriteAsJsonAsync(
                new { ressult = 0, messageErr = "Ошибка десериализации переданного JSON!", errCode = 202 });
        }
    }
    catch (ArgumentException ex)
    {
        // Поидее текст exception нельзя возвращать, вдруг чего лишнего там находится
        // но ни кода ошибки нет, ничего другого за что зацепится при вставке сущ-го Id
        await response.WriteAsJsonAsync(new { result = 0, errMessage = ex.Message, errCode = 203 });
    }
    catch (Exception)
    {
        await response.WriteAsJsonAsync(new { result = 0, errMessage = "Некорректные данные", errCode = 204 });
    }
}

async Task GetEntity(string paramValue, HttpResponse response)
{
    try
    {
        if (Guid.TryParse(paramValue, out Guid id))
        {
            using (var context = new AppDbContext())
            {
                var entityRepository = new EntityRepository(context);
                var entity = await entityRepository.GetAsync(id);
                if (entity != null)
                    await response.WriteAsJsonAsync(entity);
                else
                    await response.WriteAsJsonAsync(new { });
            }
        }
        else
        {
            await response.WriteAsJsonAsync(new { result = 0, errMessage = "Не верный Id!", errCode = 301 });
        }
    }
    catch (Exception)
    {
        await response.WriteAsJsonAsync(new { result = 0, errMessage = "Некорректные данные", errCode = 302 });
    }
}