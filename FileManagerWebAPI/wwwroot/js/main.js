const rootPath = "C:\\";
//todo: нужно придумать решение получше, что бы для одной икноки можно было назначить несоклько расширений
const iconMap = new Map([
  ["folder", "folder"],
  [".xlsx", "file-excel"],
  [".xls", "file-excel"],
  [".pdf", "file-pdf"],
]);

const fileManagers = document.getElementById("file-managers");
const firstFileManager = document.getElementById("first-file-manager");
const selectElement = document.querySelector(".select-drive");
getDrives();

const currentPathElement = document.querySelector(".current-path");
currentPathElement.textContent = rootPath;
var content = document.querySelector(".content-list");
fillFileManager(rootPath, content);

//const table = document.getElementById("folder-content");

function removeAllListeners(element) {
  element.replaceWith(element.cloneNode(false));
}

function fillTable(data, tableContent) {
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
        cell.insertAdjacentHTML(
          "afterbegin",
          `<i class="fa fa-${iconName}"></i>`,
        );
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
  let type = cell.getAttribute("fileType");
  let path = cell.textContent;
  if (path == undefined) return;
  let currentPathElement = getCurrentPathElem(cell);
  let currentPath = currentPathElement.textContent;
  let fullPath = currentPath.endsWith("\\")
    ? currentPath + path
    : currentPath + "\\" + path;
  if (type == "folder") {
    const tableContent = getContentDiv(cell);
    fillFileManager(fullPath, tableContent).catch(alert);
  } else {
    runFile(fullPath);
  }
}

async function fillFileManager(path, tableContent) {
  let response = await fetch(
    `/api/SystemInfo/directorycontent?path=${encodeURIComponent(path)}`,
  );

  if (response.status == 200) {
    let data = await response.json();
    fillTable(data, tableContent);
    let currentPathElement = getCurrentPathElem(tableContent);
    currentPathElement.textContent = path;
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

function onButtonUpLevelClick(btn) {
  const content = getContentDiv(btn);
  let currentPathElement = getCurrentPathElem(btn);
  let currentPath = currentPathElement.textContent;

  fetch(`/api/SystemInfo/ownerpath?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.text())
    .then((ownerPath) => {
      fillFileManager(ownerPath, content).catch(alert);
    })
    .catch((error) => {
      console.log(error.message);
    });
}

function onDrivesSelectChanged(option) {
  const tableContent = getContentDiv(option);
  fillFileManager(option.value, tableContent);
}

function fillDrivesSelect(jsonData) {
  //подписка на изменение списка
  selectElement.addEventListener("change", (event) => {
    onDrivesSelectChanged(event.target);
  });

  jsonData.forEach((data) => {
    const option = document.createElement("option");
    option.value = data;
    option.textContent = data;
    selectElement.append(option);
  });
}

function getParentFileManager(element) {
  return element.closest(".file-manager");
}

function getContentDiv(element) {
  let fileManager = getParentFileManager(element);
  return fileManager.querySelector(".content-list");
}

function getCurrentPathElem(element) {
  let fileManager = getParentFileManager(element);
  return fileManager.querySelector(".current-path");
}

function copy() {}
