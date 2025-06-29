<template>
    <div>
        <div class="text-2xl font-bold mb-4">
            Room Management - Day View
        </div>
        <div class="mb-4">
            <Button label="Calendar" icon="ic:outline-calendar-month" @click="onClickCalendar" />
        </div>
        <DayPilotCalendar :config="config" ref="calendarRef" />
        <!-- <ModalTest /> -->
        <EventForm ref="eventFormRef" :eventInfo="event" v-if="isEventFormShown" @update="loadEvents" />
    </div>
</template>

<script setup lang="ts">
import { DayPilot, DayPilotCalendar } from '@daypilot/daypilot-lite-vue'
import { ref, onMounted } from 'vue'
import { ModalTest } from '#components'

const route = useRoute()
const event = ref()
const isEventFormShown = ref(false)
const eventFormRef = ref()

const scroll = () => {
    isEventFormShown.value = true
    setTimeout(() => {
        console.log(eventFormRef.value)
        eventFormRef.value.$el.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }, 200)
}

const config = ref({
    viewType: 'Resources',
    locale: 'en-us',
    headerHeight: 30,
    cellHeight: 30,
    businessBeginsHour: 6,
    businessEndsHour: 21,
    timeRangeSelectedHandling: 'Enabled',
    onTimeRangeSelected: async (args) => {
        console.log(args)
        isEventFormShown.value = false
        setTimeout(() => {
            event.value = {}
            event.value.startAt = args.start
            event.value.endAt = args.end
            event.value.roomId = args.resource
            isEventFormShown.value = true
            args.control.clearSelection()
            scroll()
        }, 200)
    },
    eventDeleteHandling: 'Update',
    onEventDeleted: (args) => {
        console.log('Event deleted: ' + args.e.text())
        $fetch(`/api/v1/Event?id=${args.e.id()}`, {
            server: false,
            method: 'DELETE',
            onResponse({ response }) {
                if (!response.ok) {
                    alert(`Could not delete event ${args.e.text()}.`)
                }
                loadEvents()
            }
        })
    },
    eventMoveHandling: 'Update',
    onEventMoved: (args) => {
        $fetch(`/api/v1/Event?id=${args.e.data.id}`, {
            server: false,
            onResponse({ response }) {
                let originalEvent = response._data
                originalEvent.startAt = args.newStart
                originalEvent.endAt = args.newEnd
                originalEvent.roomId = args.newResource
                $fetch('/api/v1/Event', {
                    server: false,
                    method: 'PUT',
                    body: originalEvent,
                    onResponse({ response }) {
                        if (!response.ok) {
                            alert(response._data)
                        } else {
                            alert(`Event #${originalEvent.id} was moved successfully!`)
                        }

                        loadEvents()
                    }
                })
            }
        })
    },
    eventResizeHandling: 'Update',
    onEventResized: (args) => {
        $fetch(`/api/v1/Event?id=${args.e.data.id}`, {
            server: false,
            onResponse({ response }) {
                let originalEvent = response._data
                originalEvent.startAt = args.newStart
                originalEvent.endAt = args.newEnd
                // originalEvent.roomId = args.newResource
                $fetch('/api/v1/Event', {
                    server: false,
                    method: 'PUT',
                    body: originalEvent,
                    onResponse({ response }) {
                        if (!response.ok) {
                            alert(response._data)
                        } else {
                            alert(`Event #${originalEvent.id} was resized successfully!`)
                        }

                        loadEvents()
                    }
                })
            }
        })
    },
    eventClickHandling: 'ContextMenu',
    eventRightClickHandling: 'ContextMenu',
    contextMenu: new DayPilot.Menu({
        items: [
            {
                text: 'Edit', onClick: (args) => {
                    let id = args.source.id()
                    isEventFormShown.value = false
                    $fetch(`/api/v1/Event?id=${id}`, {
                        server: false,
                        onResponse({ response }) {
                            event.value = response._data
                            setTimeout(() => {
                                isEventFormShown.value = true
                                scroll()
                            }, 200)
                        }
                    })
                }
            },
            {
                text: 'Delete', onClick: (args) => {
                    const dp = args.source.calendar
                    dp.events.remove(args.source)
                    // console.log('Deleted through menu')
                    console.log(args)
                    $fetch(`/api/v1/Event?id=${args.source.id()}`, {
                        server: false,
                        method: 'DELETE',
                        onResponse({ response }) {
                            if (!response.ok) {
                                alert(`Could not delete event ${args.source.text()}.`)
                            }
                            loadEvents()
                        }
                    })
                }
            }
        ]
    }),
})

const calendarRef = ref(null)

const loadEvents = () => {
    let events = []
    let date = route.query.date
    isEventFormShown.value = false
    config.value.startDate = new Date(date)
    $fetch(`/api/v1/Event/Events?start=${date}&end=${date}`, {
        server: false,
        onResponse({ response }) {
            for (let event of response._data) {
                let eventStart = new Date(event.startAt)
                let eventEnd = new Date(event.endAt)
                events.push({
                    id: event.id,
                    start: eventStart.setHours(eventStart.getHours() - 7),
                    end: eventEnd.setHours(eventEnd.getHours() - 7),
                    text: event.name,
                    resource: event.roomId
                })
            }
            config.value.events = events
        }
    })
}

const loadResources = () => {
    $fetch('/api/v1/Room/Rooms', {
        server: false,
        onResponse({ response }) {
            config.value.columns = response._data
        }
    })
}

const onClickCalendar = () => navigateTo('/')

onMounted(async () => {
    loadEvents()        // load events
    loadResources()     // load rooms (resources)

    const overlay = useOverlay()
    const modal = overlay.create(ModalTest)

    const instance = modal.open()
    console.log(modal)
    console.log(instance)
    const shouldIncrement = await instance.result
})
</script>
