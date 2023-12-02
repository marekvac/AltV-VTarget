let active;
let useSroll = true;
let disableClick = false;
let level = 0;
let global_options = [];
const wrapper = $('#options-wrapper');
const child_buffer = $("#children-buffer");
const o_template = $('#option');
const eye = $('#eye');

class VOption {
	ID;
	icon;
	label;
	description;
	background;
	interact;
	children;
}

function resetView() {
	active = null;
	useSroll = true;
	disableClick = false;
	level = 0;
	global_options = [];
	wrapper.removeClass("use-click");
	// wrapper.html('<div id="response-wrapper"></div>')
	wrapper.children().not('#response-wrapper').map((i,e) => {
		$(e).remove();
	});
	child_buffer.html('');
	wrapper.append('<div class="options-list" level="0"></div>');
}

document.addEventListener("wheel", function (e) {
	if (!useSroll) return;
	disableClick = true;
	if (!active) {
		active = wrapper.children().last().children().first();
		active.addClass("active");
	} else {
		if (parseInt(e.deltaY) > 0) {
			selectDown();
		} else {
			scrollUp();
		}
	}
	return false;
}, true);

function selectDown() {
	active.removeClass("active");
	if (active.is(":last-child")) {
		active = active.parent().children().first();
	} else {
		active = active.next();
	}
	active.addClass("active");
}

function scrollUp() {
	active.removeClass("active");
	if (active.is(":first-child")) {
		active = active.parent().children().last();
	} else {
		active = active.prev();
	}
	active.addClass("active");
}

$(document).on("contextmenu", (e) => {
	// console.log(e.which);
	e.preventDefault();
});

$(document).on("mousedown", (e) => {
	if (!active && !disableClick && useSroll) {
		useSroll = false;
		wrapper.addClass("use-click");
		alt.emit("showCursor");
	}

	if (!useSroll) return;

	if (e.which == 1) {
		alt.emit("clicked", active.attr("id"));
		// console.log("Triggered callback for " + active.attr("id"));
		
		if (active.hasClass("disabled")) return;

		if (active.attr('children')) {
			let c = active.attr('children');
			if ($('#children-' + c)) {
				wrapper.append('<div class="options-list"></div>');
				const list = wrapper.children().last();
				list.append($('#children-' + c).html());
				level++;
				active = null;
			}
		}
	} else if (e.which == 3 && level > 0) {
		wrapper.children().last().remove();
		level--;
		active = wrapper.children().last().children(".active");
	}
});

$(document).on('click', '.option', function () {
	if (useSroll || disableClick) return;
	$(this).siblings().map(function (i, o) {
		$(o).removeClass("active");
	})
	let lev = parseInt($(this).parent().attr("level"));
	if (level > lev) {
		for (let i = level; i > lev; i--) {
			const opt = wrapper.children().get(i+1);
			$(opt).remove();
			level--;
		}
	}
	alt.emit("clicked", $(this).attr("id"));
	// console.log("Triggered callback for " + $(this).attr("id"));
	
	if ($(this).hasClass("disabled")) return;

	if ($(this).attr('children')) {
		$(this).addClass("active");
		let c = $(this).attr('children');
		if ($('#children-' + c)) {
			level++;
			wrapper.append('<div class="options-list" level="' + level + '"></div>');
			const list = wrapper.children().last();
			list.append($('#children-' + c).html());
		}
	}
});

function parseOption(o) {
	const option = new VOption();
	option.ID = o[0];
	option.icon = o[1];
	option.label = o[2];
	option.description = o[3];
	option.background = o[4];
	option.interact = o[5];
	option.position = o[6];
	if (o[7].length > 0) {
		option.children = [];
		o[7].forEach(o2 => {
			option.children.push(parseOption(o2));
		});
	}
	return option;
}

// function renderOptions(options) {
// 	const parent = wrapper.children().get(1);
// 	options.forEach(o => {
// 		const e = o_template.clone();
// 		e.attr("id", o.ID);
// 		e.find("i").addClass(o.icon);
// 		e.find("span").html(o.label);
// 		if (!o.interact) {
// 			e.addClass("disabled");
// 		}
// 		if (o.background) {
// 			e.addClass(o.background);
// 		}
// 		if (o.children && o.children.length > 0) {
// 			e.append('<i class="fas fa-chevron-right haschild"></i>');
// 			e.attr('children', o.ID);
// 			renderChildren(o);
// 		}
// 		$(parent).append(e);
// 	});
// }

function renderChildren(option) {
	child_buffer.append('<div id="children-' + option.ID + '"></div>');
	const parent = $('#children-' + option.ID);
	option.children.forEach(c => {
		const e = o_template.clone();
		e.attr("id", c.ID);
		e.find("i").addClass(c.icon);
		e.find("span").html(c.label);
		if (!c.interact) {
			e.addClass("disabled");
		}
		if (c.background) {
			e.addClass(c.background);
		}
		if (c.children && c.children.length > 0) {
			e.append('<i class="fas fa-chevron-right haschild"></i>');
			e.attr('children', c.ID);
			renderChildren(c);
		}
		parent.append(e);
	});
}

function startTargeting() {
	eye.removeClass("active");
	resetView();
	$('html').addClass("show");
}

function hit() {
	eye.addClass("active");
}

function hitLeft() {
	eye.removeClass("active");
	// resetView();
}

function stopTargeting() {
	$('html').removeClass("show");
	$('#response-wrapper').html('');
}

// function registerOptions(...options) {
// 	resetView();
// 	const optionss = [];
// 	options.forEach(o => {
// 		optionss.push(parseOption(o));
// 	});
// 	renderOptions(optionss);
// }

function addOption(...option) {
	const o = parseOption(option);
	global_options[o.ID] = o;
	const parent = wrapper.children().get(1);
	
	const e = o_template.clone();
	e.attr("id", o.ID);
	e.find("i").addClass(o.icon);
	e.find("span").html(o.label);
	if (!o.interact) {
		e.addClass("disabled");
	}
	if (o.background) {
		e.addClass("bg-" + o.background);
	}
	if (o.children && o.children.length > 0) {
		e.append('<i class="fas fa-chevron-right haschild"></i>');
		e.attr('children', o.ID);
		renderChildren(o);
	}
	$(parent).append(e);
}

function removeChildren(o) {
	$("#children-" + o.ID).remove();
	if (o.children && o.children.length) {
		o.children.forEach(c => {
			removeChildren(c);
		})
	}
}

function removeOption(name) {
	const parent = wrapper.children().get(1);
	const e = $(parent).find("[id='" + name + "']");
	
	if (global_options[name]) {
		const o = global_options[name];
		if (o.children && o.children.length) {
			removeChildren(o);
		}
		delete global_options[name];
	}

	if (e.hasClass("active")) {
		active = null;
		if (e.attr("children")) {
			let lev = parseInt(e.parent().attr("level"));
			if (level > lev) {
				for (let i = level; i > lev; i--) {
					const opt = wrapper.children().get(i+1);
					$(opt).remove();
					level--;
				}
			}
		}
	}
	e.removeClass("show");
	setTimeout(() => {
		e.remove();
	},100);
	// e.remove();
}

let timeout;
function alertResponse(bg, message) {
	$('#response-wrapper').html('');
	$('#response-wrapper').append('<div class="option bg-' + bg + '">	<span>' + message + '</span></div>');
	$('#response-wrapper').children().first().addClass('show');
	clearTimeout(timeout);
	timeout = setTimeout(() => {
		$('#response-wrapper').children().first().removeClass('show');
	}, 1500);
}

alt.on("startTargeting", startTargeting);
alt.on("hit", hit);
alt.on("hitLeft", hitLeft);
alt.on("stopTargeting", stopTargeting);
alt.on("clearOptions", resetView);
alt.on("alert", alertResponse);
alt.on("addOption", addOption);
alt.on("removeOption", removeOption);