//todo: нужно придумать решение получше, что бы для одной икноки можно было назначить несоклько расширений
const iconMap = new Map([
  ["dir", "folder"],
  [".xlsx", "file-excel"],
  [".xls", "file-excel"],
  [".pdf", "file-pdf"],
]);
const fileManagers = document.getElementById("file-managers-container");
const firstFileManager = document.querySelector(".file-manager");

const secondFileManager = firstFileManager.cloneNode(true);
fileManagers.append(secondFileManager);
const firstContent = getContentDivByFileManager(firstFileManager);
const secondContent = getContentDivByFileManager(secondFileManager);
const directionSelect = document.getElementById("select-direction");
fillFileManagers();

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

        cell.classList.add("name");
        cell.style.cursor = "pointer";
        let iconName = iconMap.has(item.type) ? iconMap.get(item.type) : "file";
        //добавляется соответствующая иконка к названию файла
        cell.insertAdjacentHTML(
          "afterbegin",
          `<i class="fa fa-${iconName}"></i>`,
        );
        cell.setAttribute("fileType", item.type);
        cell.style.width = "auto";
      } else {
        //cell.style.textAlign = "right";
        //cell.style.width = "60px";
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
}
function onContentItemDblClick(cell) {
  let type = cell.getAttribute("fileType");
  let elementName = cell.textContent;
  if (elementName == undefined) return;

  let currentPathElement = getCurrentPathElem(cell);
  let currentPath = currentPathElement.textContent;
  let fullPath = getFullPath(currentPath, elementName);

  if (type == "dir") {
    const tableContent = getContentDiv(cell);
    refreshFileManagerContent(fullPath, tableContent).catch(alert);
  } else {
    runFile(fullPath);
  }
}

async function refreshFileManagerContent(path, tableContent) {
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

async function fillFileManagers() {
    //todo: сделать await, т.к.контент нужно заполнить после того как отработает promise
    try {
        const response = await fetch("/api/SystemInfo/harddrives");
        const data = await response.json();
        fillDrivesSelect(data);
        refreshFileManagerContent(data[0], firstContent);
        refreshFileManagerContent(data[0], secondContent);
    } catch (error) {
        console.error(error.message);
    }
}

function onButtonUpLevelClick(btn) {
  const content = getContentDiv(btn);
  let currentPathElement = getCurrentPathElem(btn);
  let currentPath = currentPathElement.textContent;

  fetch(`/api/SystemInfo/ownerpath?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.text())
    .then((ownerPath) => {
      refreshFileManagerContent(ownerPath, content).catch(alert);
    })
    .catch((error) => {
      console.log(error.message);
    });
}

function onDrivesSelectChanged(option) {
  const tableContent = getContentDiv(option);
  refreshFileManagerContent(option.value, tableContent);
}

function fillDrivesSelect(drives) {
  const driveSelectElements = document.querySelectorAll(".select-drive");
  driveSelectElements.forEach((driveSelectElement) => {
    //подписка на изменение списка
    driveSelectElement.addEventListener("change", (event) => {
      onDrivesSelectChanged(event.target);
    });

    drives.forEach((data) => {
      const option = document.createElement("option");
      option.value = data;
      option.textContent = data;
      driveSelectElement.append(option);
    });
  });
}

function getParentFileManager(element) {
  return element.closest(".file-manager");
}

function getContentDiv(element) {
  let fileManager = getParentFileManager(element);
  return getContentDivByFileManager(fileManager);
}

function getContentDivByFileManager(fileManager) {
  return fileManager.querySelector(".content-list");
}

function getCurrentPathElem(element) {
  let fileManager = getParentFileManager(element);
  return getCurrentPathElemByFileManager(fileManager);
}

function getCurrentPathElemByFileManager(fileManager) {
  return fileManager.querySelector(".current-path");
}

function getCurrentPathByFileManager(fileManager) {
  let element = getCurrentPathElemByFileManager(fileManager);
  return element.textContent;
}

function getFullPath(path, elementName) {
  return path.endsWith("\\") ? path + elementName : path + "\\" + elementName;
}

function getSelectedNames(fileManager) {
  let selectedItems = fileManager.querySelectorAll(".selected-row");

  let seletedNames = Array.from(selectedItems).map((item) => {
    var pathCell = item.querySelector(".name");
    return pathCell.textContent;
  });
  return seletedNames;
}

//перемещение/копирование выделенных элементов
function transferElements(btn, isCopy) {
  let transferFromMngr = getParentFileManager(btn);
  let transferToMngr =
    transferFromMngr == firstFileManager ? secondFileManager : firstFileManager;
  let currentPathFrom = getCurrentPathByFileManager(transferFromMngr);
  let currentPathTo = getCurrentPathByFileManager(transferToMngr);
  let selectedNames = getSelectedNames(transferFromMngr);

  fetch("/api/SystemInfo/transferelements", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      NameCollection: selectedNames,
      SourcePath: currentPathFrom,
      DestinationPath: currentPathTo,
      IsCopy: isCopy,
    }),
  })
    .then((response) => {
      if (!response.ok) {
        throw new Error("HTTP error " + response.status);
      }

      refreshFileManagerContent(
        getCurrentPathByFileManager(firstFileManager),
        getContentDivByFileManager(firstFileManager),
      );
      refreshFileManagerContent(
        getCurrentPathByFileManager(secondFileManager),
        getContentDivByFileManager(secondFileManager),
      );
    })
    .catch((error) => {
      console.log(error.message);
    });
}

function deleteElements(btn) {
  let deleteFromMngr = getParentFileManager(btn);
  let currentPath = getCurrentPathByFileManager(deleteFromMngr);
  let selectedNames = getSelectedNames(deleteFromMngr);

  fetch("/api/SystemInfo/deleteelements", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      NameCollection: selectedNames,
        SourcePath: currentPath,
    }),
  })
    .then((response) => {
      if (!response.ok) {
        throw new Error("HTTP error " + response.status);
      }

      refreshFileManagerContent(
        getCurrentPathByFileManager(deleteFromMngr),
        getContentDivByFileManager(deleteFromMngr),
      );
    })
    .catch((error) => {
      console.log(error.message);
    });
}
