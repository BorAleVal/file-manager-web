const rootPath = "C:\\";
var currentPath = rootPath;
//todo: нужно придумать решение получше, что бы для одной икноки можно было назначить несоклько расширений
const iconMap = new Map([
  ["folder", "folder"],
  [".xlsx", "file-excel"],
  [".xls", "file-excel"],
  [".pdf", "file-pdf"],
]);

const selectElement = document.getElementById("select-drive");
const currentPathElement = document.getElementById("current-path");
currentPathElement.textContent = currentPath;
getDrives();
fillFileManager(rootPath);

const table = document.getElementById("folder-content");
const tableContent = document.getElementById("folder-content-list");

function removeAllListeners(element) {
  element.replaceWith(element.cloneNode(false));
}

function fillTable(data) {
  while (tableContent.childNodes.length) {
    //на сколько понимаю нужно убрать всех подписчиков, во избежание утечки памяти
    removeAllListeners(tableContent.childNodes[0]);
    tableContent.removeChild(tableContent.childNodes[0]);
  }

  // Заполнение таблицы данными
  data.forEach((item) => {
    const row = tableContent.insertRow();
    //для выделения строки
    row.addEventListener("click", function () {
      onContentRowClick(row);
    });

    Object.entries(item).forEach(([key, value]) => {
      if (key == "type") return;

      const cell = row.insertCell();
      cell.textContent = value;
      if (key == "name") {
        cell.addEventListener("dblclick", function () {
          onContentItemDblClick(cell);
        });
        row.setAttribute("name", item.name);
        cell.style.cursor = "pointer";
        let iconName = iconMap.has(item.type) ? iconMap.get(item.type) : "file";
        //добавляется соответствующая иконка к названию файла
        //https://habr.com/ru/companies/timeweb/articles/843080/
        cell.insertAdjacentHTML("afterbegin", `<i class="fa fa-${iconName}"></i>`);
        cell.setAttribute("fileType", item.type);
      } else {
        cell.style.textAlign = "right";
        cell.style.width = "60px";
      }
    });
  });
}



function onContentRowClick(row) {
  if (row.classList.contains("selected-row")) {
    row.classList.remove("selected-row");
  } else {
    row.classList.add("selected-row");
  }

  getSelectedRows().forEach((row) => {
    console.log(row.getAttribute("name"));
  });
}

function getSelectedRows() {
  return document.querySelectorAll(".selected-row");
}

function onContentItemDblClick(cell) {
  console.log("fillFileManagerBtn");
  let type = cell.getAttribute("fileType");
  let path = cell.textContent;
  if (path == undefined) return;
  let fullPath = currentPath.endsWith("\\")
    ? currentPath + path
    : currentPath + "\\" + path;
  if (type == "folder") {
    fillFileManager(fullPath).catch(alert);
  } else {
    runFile(fullPath);
  }
}

async function fillFileManager(path) {
  let response = await fetch(
    `/api/SystemInfo/directorycontent?path=${encodeURIComponent(path)}`,
  );

  if (response.status == 200) {
    let data = await response.json();
    fillTable(data);
    currentPath = path;
    currentPathElement.textContent = currentPath;
    return;
  }
  let data = await response.json();
  throw new Error(data.message);
}

function runFile(path) {
  fetch(`/api/SystemInfo/runfile?filepath=${encodeURIComponent(path)}`).catch(
    (error) => {
      console.log(error.message);
    },
  );
}

function getDrives() {
  fetch("/api/SystemInfo/harddrives")
    .then((response) => response.json())
    .then((data) => {
      fillDrivesSelect(data);
      return true;
    })
    .catch((error) => {
      console.log(error.message);
    });
}

function onButtonUpLevelClick() {
  fetch(`/api/SystemInfo/ownerpath?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.text())
    .then((ownerPath) => {
      fillFileManager(ownerPath).catch(alert);
    })
    .catch((error) => {
      console.log(error.message);
    });
}

function onDrivesSelectChanged(text) {
  fillFileManager(text);
}

function fillDrivesSelect(jsonData) {
  //подписка на изменение списка
  selectElement.addEventListener("change", (event) => {
    onDrivesSelectChanged(event.target.value);
  });

  jsonData.forEach((data) => {
    const option = document.createElement("option");
    option.value = data;
    option.textContent = data;
    selectElement.appendChild(option);
  });
}

function copy() {}
