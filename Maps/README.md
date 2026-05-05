# Maps assets

Папка читается `MuAdmin.Core.Assets.AssetsManager.GetMap(int)` и
используется редактором спотов (`Monsters/MonsterSetBase*.txt`) для
отрисовки превью карты с прямоугольниками зон спавна.

## Имена файлов

`<MapName>.{jpg,jpeg,png,bmp}`, где `MapName` — каноническое имя
из `MuAdmin.Core.Catalogs.MapNames` (`Lorencia`, `Devias`, `Atlans`,
`Tarkan`, `Aida`, `Crywolf`, `Kalima1`–`Kalima6`, `Kantru`,
`Stadium`, `Vulcano`, `BarracksOfBalgass`, и т.д.). Расширение и
регистр базового имени значения не имеют.

Если файла нет — менеджер ассетов сгенерирует аккуратный пастельный
плейсхолдер с подписью.

## Легальность

Не коммитьте сюда официальные ассеты Webzen. Используйте мини-карты
со своего собственного клиента (например, экспорт из
`Data/World*/EncTerrain*.att`) или собственные изображения.
