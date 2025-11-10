document.addEventListener('DOMContentLoaded', () => {
    if (window.lucide && window.lucide.createIcons) {
        window.lucide.createIcons();
    }
    initializeCalendarPage();
});

// ... existing code ...
function initializeCalendarPage() {
    const calendarEl = document.getElementById('calendar');
    if (!calendarEl || !window.FullCalendar) return;

    const eventDetailPopover = document.getElementById('eventDetailPopover');
    const createEventPopover = document.getElementById('createEventPopover');

    const PROJECT_COLORS = { 'Project-1': '#2D5B9A', 'Project-2': '#1fcf34', 'Project-3': '#ffca37', 'Project-4': '#a145ec' };
    const PROJECT_VALUE_MAP = { prj1: 'Project-1', prj2: 'Project-2', prj3: 'Project-3', prj4: 'Project-4' };
    const PROJECT_VALUE_REVERSE = { 'Project-1': 'prj1', 'Project-2': 'prj2', 'Project-3': 'prj3', 'Project-4': 'prj4' };

    let currentClickedEvent = null;
    let editingEventId = null;
    let selectedRange = { start: null, end: null };

    let eventsData = [
        {
            id: 'evt1',
            title: 'Team Meeting',
            start: new Date().toISOString().substr(0, 10) + 'T10:30:00',
            end: new Date().toISOString().substr(0, 10) + 'T12:30:00',
            extendedProps: {
                project: 'Project-1',
                color: PROJECT_COLORS['Project-1']
            }
        },
        {
            id: 'evt2',
            title: 'Lunch with Client',
            start: '2025-08-15',
            end: '2025-08-20',
            color: '#F79009',
            extendedProps: {
                project: 'Project-2',
                color: PROJECT_COLORS['Project-2']
            }
        },
        {
            id: 'evt3',
            title: 'Project Deadline',
            start: '2025-08-20',
            allDay: true,
            color: '#ff4d40',
            display: 'background'
        }
    ];

    const mainCalendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today addEventButton',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        editable: true,
        selectable: true,
        events: eventsData,
        eventClick: function (info) {
            info.jsEvent.preventDefault();
            if (!eventDetailPopover) return;

            currentClickedEvent = info.event;

            const titleEl = eventDetailPopover.querySelector('.event-popover__title strong');
            const timeEl = eventDetailPopover.querySelector('.event-popover__meta span');
            const projectEl = eventDetailPopover.querySelector('.event-popover__meta:last-child span:last-child');
            const projectDotEl = eventDetailPopover.querySelector('.event-popover__meta .project-color-dot');

            if (titleEl) titleEl.textContent = info.event.title || '';
            if (timeEl) timeEl.textContent = info.event.start ? info.event.start.toLocaleString() : '';

            if (info.event.extendedProps.project) {
                if (projectEl) projectEl.textContent = info.event.extendedProps.project;
                if (projectDotEl) projectDotEl.style.backgroundColor = info.event.extendedProps.color || '#80c8ff';
                const projMeta = projectEl ? projectEl.parentElement : null;
                if (projMeta) projMeta.hidden = false;
            } else {
                const projMeta = projectEl ? projectEl.parentElement : null;
                if (projMeta) projMeta.hidden = true;
            }

            eventDetailPopover.style.top = `${info.jsEvent.clientY}px`;
            eventDetailPopover.style.left = `${info.jsEvent.clientX}px`;
            eventDetailPopover.showPopover();

            const editBtn = eventDetailPopover.querySelector('.btn--secondary');
            if (editBtn) {
                editBtn.onclick = () => {
                    editingEventId = currentClickedEvent.id;
                    const titleInput = document.getElementById('newEventTitle');
                    const projectSelect = document.getElementById('newEventProject');
                    if (titleInput) titleInput.value = currentClickedEvent.title || '';
                    if (projectSelect) projectSelect.value = PROJECT_VALUE_REVERSE[currentClickedEvent.extendedProps.project] || 'prj1';
                    eventDetailPopover.hidePopover();
                    centerPopover(createEventPopover);
                    createEventPopover.showPopover();
                };
            }
        },
        select: function (info) {
            if (!createEventPopover) return;
            selectedRange.start = info.start;
            selectedRange.end = info.end;
            editingEventId = null;
            centerPopover(createEventPopover);
            createEventPopover.showPopover();
        },
        customButtons: {
            addEventButton: {
                text: '+ New Event',
                click: function () {
                    if (!createEventPopover) return;
                    selectedRange = { start: null, end: null };
                    editingEventId = null;
                    const titleInput = document.getElementById('newEventTitle');
                    const projectSelect = document.getElementById('newEventProject');
                    if (titleInput) titleInput.value = '';
                    if (projectSelect) projectSelect.value = 'prj1';
                    centerPopover(createEventPopover);
                    createEventPopover.showPopover();
                }
            }
        }
    });
    mainCalendar.render();

    const miniCalendarEl = document.getElementById('mini-calendar');
    if (miniCalendarEl) {
        const miniCalendar = new FullCalendar.Calendar(miniCalendarEl, {
            initialView: 'dayGridMonth',
            headerToolbar: { left: 'prev', center: 'title', right: 'next' },
            dateClick: function (info) { mainCalendar.gotoDate(info.date); }
        });
        miniCalendar.render();
    }

    function centerPopover(pop) {
        if (!pop) return;
        pop.style.top = '50%';
        pop.style.left = '50%';
        pop.style.transform = 'translate(-50%, -50%)';
    }

    function getActiveProjects() {
        const active = new Set();
        document.querySelectorAll('.project-displayed').forEach(cb => {
            const labelEl = cb.parentElement?.querySelector('.item-filter-name');
            const name = labelEl ? labelEl.textContent.trim() : null;
            if (cb.checked && name) active.add(name);
        });
        return active;
    }

    function filterEvents() {
        const activeProjects = getActiveProjects();
        const searchInput = document.querySelector('.filter-search-bar .search-bar__input');
        const q = (searchInput?.value || '').toLowerCase();

        const filtered = eventsData.filter(ev => {
            const projOk = ev.extendedProps?.project ? activeProjects.has(ev.extendedProps.project) : true;
            const text = (ev.title || '').toLowerCase() + ' ' + (ev.extendedProps?.project || '').toLowerCase();
            const searchOk = q ? text.includes(q) : true;
            return projOk && searchOk;
        });

        mainCalendar.removeAllEvents();
        filtered.forEach(e => mainCalendar.addEvent(e));
    }

    const searchInputEl = document.querySelector('.filter-search-bar .search-bar__input');
    if (searchInputEl) searchInputEl.addEventListener('input', filterEvents);
    document.querySelectorAll('.project-displayed').forEach(cb => cb.addEventListener('change', filterEvents));

    document.querySelectorAll('.toggle-workspace-list-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const targetId = btn.dataset.target;
            const listEl = document.getElementById(targetId);
            const expanded = btn.getAttribute('aria-expanded') === 'true';
            btn.setAttribute('aria-expanded', expanded ? 'false' : 'true');
            if (listEl) listEl.style.display = expanded ? 'none' : '';
            const icon = btn.querySelector('.icon-toggle');
            if (icon) icon.setAttribute('data-open', expanded ? 'false' : 'true');
        });
    });

    const toggleAllOne = document.getElementById('show-all-projects-btn');
    if (toggleAllOne) {
        toggleAllOne.addEventListener('change', e => {
            const checked = e.target.checked;
            document.querySelectorAll('#prjs-wsp-1 .project-displayed').forEach(cb => { cb.checked = checked; });
            filterEvents();
        });
    }
    const toggleAllTwo = document.getElementById('show-all-projects-btn-2');
    if (toggleAllTwo) {
        toggleAllTwo.addEventListener('change', e => {
            const checked = e.target.checked;
            document.querySelectorAll('#prjs-wsp-2 .project-displayed').forEach(cb => { cb.checked = checked; });
            filterEvents();
        });
    }

    const saveBtn = createEventPopover?.querySelector('.btn.btn--primary');
    if (saveBtn) {
        saveBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const titleInput = document.getElementById('newEventTitle');
            const projectSelect = document.getElementById('newEventProject');

            const title = titleInput?.value.trim();
            const projVal = projectSelect?.value;
            const projectName = PROJECT_VALUE_MAP[projVal || 'prj1'];

            if (!title) return;

            if (editingEventId) {
                const ev = mainCalendar.getEventById(editingEventId);
                if (ev) {
                    ev.setProp('title', title);
                    ev.setExtendedProp('project', projectName);
                    ev.setExtendedProp('color', PROJECT_COLORS[projectName]);
                }
                editingEventId = null;
            } else {
                const newEvent = {
                    id: 'evt_' + Date.now(),
                    title,
                    start: selectedRange.start ? selectedRange.start.toISOString() : new Date().toISOString().substr(0, 10),
                    end: selectedRange.end ? selectedRange.end.toISOString() : null,
                    extendedProps: {
                        project: projectName,
                        color: PROJECT_COLORS[projectName]
                    }
                };
                eventsData.push(newEvent);
                mainCalendar.addEvent(newEvent);
            }

            createEventPopover.hidePopover();
            if (titleInput) titleInput.value = '';
        });
    }

    filterEvents();
}
// ... existing code ...