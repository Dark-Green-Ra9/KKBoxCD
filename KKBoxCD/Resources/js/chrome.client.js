const ListSRP = {};

function GetSRP(name) {
    if (ListSRP[name] == null) {
        ListSRP[name] = new SRP();
    }
    return ListSRP[name];
}

function RemoveSRP(name) {
    delete ListSRP[name];
}

function GetPlanFromHTML(html) {
    try {
        const root = document.createElement("div");
        root.innerHTML = html;

        if (root.querySelector(".no_plan") != null) {
            return "No Plan";
        }

        const card = root.querySelector(".plan_card");
        const data = card.innerText.trim().split("\n");
        var plan = "";

        if (data.length > 0) {
            plan += data[0].trim().concat(" | ");
        }
        if (data.length > 1) {
            plan += data[1].trim().concat(" | ");
        }
        if (data.length > 9) {
            plan += data[9].trim().concat(" | ");
        }
        if (data.length > 25) {
            plan += data[25].trim();
        }
        root.remove();

        return plan;
    } catch {
        return "Get Failed";
    }
}