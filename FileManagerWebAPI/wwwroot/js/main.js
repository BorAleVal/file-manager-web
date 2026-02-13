//Буду благодарен за ОС



//todo: нужно придумать решение получше, что бы для одной икноки можно было назначить несоклько расширений
const iconMap = new Map([
  ["dir", "folder"],
  [".xlsx", "file-excel"],
  [".xls", "file-excel"],
  [".pdf", "file-pdf"],
]);
const panels = document.getElementById("panels-container");
const panel1 = document.querySelector(".panel");
//добавляется вторая панель посредством копирования первой
const panel2 = panel1.cloneNode(true);
panels.append(panel2);

const content1 = getContentDivByPanel(panel1);
const content2 = getContentDivByPanel(panel2);

fillPanels();

async function fillPanels() {
  try {
    const response = await fetch("/api/SystemInfo/harddrives");
    const data = await response.json();
    fillDrivesSelect(data);
    refreshPanelContent(data[0], content1);
    refreshPanelContent(data[0], content2);
  } catch (error) {
    console.error(error.message);
  }
}

function removeAllListeners(element) {
  element.replaceWith(element.cloneNode(false));
}

function clearTable(tableContent) {
  while (tableContent.childNodes.length) {
    //на сколько понимаю нужно убрать всех подписчиков, во избежание утечки памяти
    removeAllListeners(tableContent.childNodes[0]);
    tableContent.removeChild(tableContent.childNodes[0]);
  }
}

function fillTable(data, tableContent) {
  clearTable(tableContent);
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
        const iconName = iconMap.has(item.type)
          ? iconMap.get(item.type)
          : "file";
        //добавляется соответствующая иконка к названию файла
        cell.insertAdjacentHTML(
          "afterbegin",
          `<i class="fa fa-${iconName}"></i>`,
        );
        cell.setAttribute("fileType", item.type);
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
  const type = cell.getAttribute("fileType");
  const elementName = cell.textContent;
  if (elementName == undefined) return;

  const currentPathElement = getCurrentPathElem(cell);
  const currentPath = currentPathElement.textContent;
  const fullPath = getFullPath(currentPath, elementName);

  if (type == "dir") {
    const tableContent = getContentDiv(cell);
    refreshPanelContent(fullPath, tableContent).catch(alert);
  } else {
    runFile(fullPath);
  }
}

async function refreshPanelContent(path, tableContent) {
  const response = await fetch(
    `/api/SystemInfo/directorycontent?path=${encodeURIComponent(path)}`,
  );

  if (response.ok) {
    const data = await response.json();
    fillTable(data, tableContent);
    const currentPathElement = getCurrentPathElem(tableContent);
    currentPathElement.textContent = path;
    return;
  }
  const data = await response.json();
  throw new Error(data.message);
}

function runFile(path) {
  fetch(`/api/SystemInfo/runfile?filepath=${encodeURIComponent(path)}`).catch(
    (error) => {
      console.log(error.message);
    },
  );
}

function onButtonUpLevelClick(btn) {
  const content = getContentDiv(btn);
  const currentPathElement = getCurrentPathElem(btn);
  const currentPath = currentPathElement.textContent;

  fetch(`/api/SystemInfo/ownerpath?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.text())
    .then((ownerPath) => {
      refreshPanelContent(ownerPath, content).catch(alert);
    })
    .catch((error) => {
      console.log(error.message);
    });
}

function onDrivesSelectChanged(option) {
  const tableContent = getContentDiv(option);
  refreshPanelContent(option.value, tableContent);
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

function getParentPanel(element) {
  return element.closest(".panel");
}

function getContentDiv(element) {
  const panel = getParentPanel(element);
  return getContentDivByPanel(panel);
}

function getContentDivByPanel(panel) {
  return panel.querySelector(".content-list");
}

function getCurrentPathElem(element) {
  const panel = getParentPanel(element);
  return getCurrentPathElemByPanel(panel);
}

function getCurrentPathElemByPanel(panel) {
  return panel.querySelector(".current-path");
}

function getCurrentPathByPanel(panel) {
  const element = getCurrentPathElemByPanel(panel);
  return element.textContent;
}

function getFullPath(path, elementName) {
  return path.endsWith("\\") ? path + elementName : path + "\\" + elementName;
}

function getSelectedNames(panel) {
  const selectedItems = panel.querySelectorAll(".selected-row");

  const seletedNames = Array.from(selectedItems).map((item) => {
    const pathCell = item.querySelector(".name");
    return pathCell.textContent;
  });
  return seletedNames;
}

//перемещение/копирование выделенных элементов (так и не придумал адекватного имени метода)
async function transferElements(btn, isCopy) {
  const transferFromMngr = getParentPanel(btn);
  const transferToMngr = transferFromMngr == panel1 ? panel2 : panel1;
  const currentPathFrom = getCurrentPathByPanel(transferFromMngr);
  const currentPathTo = getCurrentPathByPanel(transferToMngr);
  const selectedNames = getSelectedNames(transferFromMngr);
  try {
    const response = await fetch("/api/SystemInfo/transferelements", {
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
    });
    if (!response.ok) {
      throw new Error("Сервер вернул ошибку: " + response.message);
    }

    refreshPanelContent(
      getCurrentPathByPanel(panel1),
      getContentDivByPanel(panel1),
    );
    refreshPanelContent(
      getCurrentPathByPanel(panel2),
      getContentDivByPanel(panel2),
    );
  } catch (error) {
    alert(error.message);
  }
}

async function deleteElements(btn) {
  const deleteFromMngr = getParentPanel(btn);
  const currentPath = getCurrentPathByPanel(deleteFromMngr);
  const selectedNames = getSelectedNames(deleteFromMngr);
  try {
    const response = await fetch("/api/SystemInfo/deleteelements", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        NameCollection: selectedNames,
        SourcePath: currentPath,
      }),
    });
    if (!response.ok) {
      // Если нет — выбрасываем ошибку вручную
      throw new Error("Сервер вернул ошибку: " + response.message);
    }

    refreshPanelContent(
      getCurrentPathByPanel(deleteFromMngr),
      getContentDivByPanel(deleteFromMngr),
    );
  } catch (error) {
    alert(error.message);
  }
}
