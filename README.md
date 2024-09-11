# FamilyCompactor

Плагин для уменьшения размеров файлов семейств Revit (rfa).

## Описание

Плагин создаёт временную копию семейства, затем загружает её в пустой проект. Далее открывает семейство из проекта для редактирования и сохраняет его. При этом размер файла может уменьшиться.
Также плагин открывает семейство в редакторе семейств и сохраняет его оттуда под новым именем. При этом размер файла также может уменьшиться.
Далее плагин выбирает тот вариант, в результате которого файл уменьшился больше, и сохраняет его под исходным именем, предварительно создав резервную копию аналогично тому, как это обычно делает Revit.

## Поддерживаемые версии Revit

2019-2024

## Использование

На вкладке **Надстройки** в группе **FamilyCompactor** нажмите кнопку **Compact families**. Откроется диалоговое окно, где в текстовом поле можно вставить список всех полных путей к семействам, размеры которых нужно попытаться уменьшить, а также можно нажать кнопку **Обзор...**, чтоб добавить их через стандартное диалоговое окно открытия файлов. После этого нажать **OK** и дождаться сообщения о завершении.

## Установка

Запустите `FamilyCompactor-1.0.0.0-Revit{версия Revit}.msi` для соответствующей версии Revit. Для установки не требуются права администратора.

## English

Plugin for reducing the size of Revit family files (rfa).

## Description

The plugin creates a temporary copy of the family, then loads it into an empty project. Then it opens the family from the project for editing and saves it. This may reduce the file size.
The plugin also opens the family in the family editor and saves it from there under a new name. This may also reduce the file size.
The plugin then selects the option that resulted in the file being smaller the most, and saves it under the original name, having previously created a backup copy similar to how Revit usually does.

## Supported Revit versions

2019-2024

## Usage

On the **Add-ins** tab, in the **FamilyCompactor** group, click the **Compact families** button. A dialog box will open where you can paste a list of all the full paths to the families whose sizes you want to try to reduce into the text box, or you can click the **Browse...** button to add them through the standard file open dialog box. Then click **OK** and wait for the completion message.

## Installation

Run `FamilyCompactor-1.0.0.0-Revit{Revit version}.msi` for the appropriate Revit version. No administrator rights are required for installation.