# WeMakeTeamTask2
В качестве БД используется Microsoft.EntityFrameworkCore.InMemory.
DateTime в БД храню в UTC, перевод в локальну тайм зону осуществляется при вставке и при считывании.
Сдела ветку где заменил DateTime на DateTimeOffset
