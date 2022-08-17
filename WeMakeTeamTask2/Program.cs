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
            new { result = 0, errMessage = "�������� ����� �������!", errCode = 1, note = $"�������� ������ {method} �����." });
        return;
    }


    if (request.Query.Count != 1)
    {
        await response.WriteAsJsonAsync(
            new { result = 0, errMessage = $"�������� ���-�� ���������� � ������� ({request.Query.Count})!", errCode = 101, 
                note = "������ ���� ���� ��������." });
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
            new { rsult = 0, errMessage = "���������� ������ �������� � �������!", errCode = 102, 
                note = "��������� ���� �� ���������� - insert, get" });
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
            // ��� ����� ������ �������� ���������� ������, �������� ���� �� ���� ���������� �� json (�������� � �������)
            string validateSetFields = entity.ValidateSetFields();
            if (!String.IsNullOrEmpty(validateSetFields))
            {
                await response.WriteAsJsonAsync(
                    new { result = 0, errMessage = $"�� ��� ���� ���� ���������: {validateSetFields}", errCode = 201, 
                        note = "��������� ������� ����� � �������." });
                return;
            }

            using (var context = new AppDbContext())
            {
                var entityRepository = new EntityRepository(context);
                await entityRepository.InsertAsync(entity);
            }

            //response.StatusCode = 201;
            // �������
            await response.WriteAsJsonAsync(
                    new { result = 1, note = "������ ���������" });
        }
        else
        {
            await response.WriteAsJsonAsync(
                new { ressult = 0, messageErr = "������ �������������� ����������� JSON!", errCode = 202 });
        }
    }
    catch (ArgumentException ex)
    {
        // ������ ����� exception ������ ����������, ����� ���� ������� ��� ���������
        // �� �� ���� ������ ���, ������ ������� �� ��� ��������� ��� ������� ���-�� Id
        await response.WriteAsJsonAsync(new { result = 0, errMessage = ex.Message, errCode = 203 });
    }
    catch (Exception)
    {
        await response.WriteAsJsonAsync(new { result = 0, errMessage = "������������ ������", errCode = 204 });
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
            await response.WriteAsJsonAsync(new { result = 0, errMessage = "�� ������ Id!", errCode = 301 });
        }
    }
    catch (Exception)
    {
        await response.WriteAsJsonAsync(new { result = 0, errMessage = "������������ ������", errCode = 302 });
    }
}