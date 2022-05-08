<template>
    <QSelect
        label="Filter by color code, tag, author, or text"
        class="q-mx-none bg-white"
        v-model="filters"
        new-value-mode="add"
        use-input
        use-chips
        dense
        multiple
        hide-dropdown-icon
        @remove="handleRemoveOption"
        inputmode="none">

        <template v-slot:prepend>
            <QIcon name="mdi-note-search-outline" />
        </template>

        <template v-if="filters.length > 0" v-slot:append>
            <QBtn flat size="md" icon="mdi-backspace-outline" @click.stop="handleClearOptions" class="q-mt-xs" />
        </template>

        <!--// The display template for the selected items //-->
        <template v-slot:selected-item="scope">
            <QChip
                v-if="scope.opt.type === 'color'"
                dense
                dark
                square
                removable
                @remove="handleRemoveOption(scope)"
                :color="scope.opt.value"
                text-color="white">
                &nbsp;&nbsp;&nbsp;&nbsp;
            </QChip>
            <QChip
                v-else-if="scope.opt.type === 'user'"
                dense
                square
                removable
                @remove="handleRemoveOption(scope)">
                <QAvatar
                    icon="mdi-account-circle-outline">
                </QAvatar>
                {{ scope.opt.label }}
            </QChip>
            <QChip
                v-else-if="scope.opt.type === 'tag'"
                dense
                square
                removable
                @remove="handleRemoveOption(scope)"
                icon="mdi-tag-outline"
                outline
                color="secondary">
                #{{ scope.opt.label }}
            </QChip>
            <QChip
                v-else-if="scope.opt.type === 'icon'"
                dense
                square
                removable
                @remove="handleRemoveOption(scope)"
                outline
                color="secondary">
                <QIcon :name="scope.opt.value" />
            </QChip>
            <QChip
                v-else-if="scope.opt.type === 'sort'"
                dense
                square
                removable
                @remove="handleRemoveOption(scope)"
                outline
                color="secondary"
                :label="scope.opt.label">
                <QIcon :name="scope.opt.value === 'ascending' ? 'mdi-arrow-up-thick' : 'mdi-arrow-down-thick'" />
            </QChip>
            <QChip
                v-else
                removable
                dense
                square
                @remove="handleRemoveOption(scope)"
                :label="JSON.stringify(scope)"/>
        </template>

        <!--// The fly-out menu //-->
        <QMenu
            fit
            cover
            anchor="top middle"
            max-width="800px"
            ref="filterMenu"
            transition-show="jump-down"
            transition-hide="jump-up"
            @hide="applyFilters">
            <QBar class="bg-white text-dark">
                <QIcon name="mdi-note-search-outline" color="dark"></QIcon>
                <div>Filter Notes</div>
                <QSpace/>
                <QBtn dense round flat icon="mdi-close" @click="hideMenu"/>
            </QBar>
            <QList>
                <QItem>
                    <QInput dense class="q-mx-none full-width" title="Keyword" v-model="searchInputs"/>
                </QItem>
                <QItem>
                    <QItemSection>
                        <QItemLabel class="text-subtitle2">Color Code</QItemLabel>
                        <QItemLabel class="q-gutter-sm">
                            <QRadio keep-color v-model="messageToSearch.color" val="orange" color="orange" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="pink" color="pink" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="purple" color="purple" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="indigo" color="indigo" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="blue" color="blue" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="cyan" color="cyan" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="teal" color="teal" size="lg" dense />
                            <QRadio keep-color v-model="messageToSearch.color" val="light-green" color="light-green" size="lg" dense />
                        </QItemLabel>
                    </QItemSection>
                </QItem>
                <QItem>
                    <QItemSection>
                        <QItemLabel class="text-subtitle2">Tags</QItemLabel>
                        <QItemLabel lines="5">
                            <QChip
                                dense
                                :outline="!tag.selected"
                                :text-color="tag.selected ? 'white' : 'secondary'"
                                square
                                clickable
                                size="lg"
                                color="secondary"
                                v-for="(tag, index) in tags"
                                :key="`tag-${index}`" ripple
                                v-model:selected="tag.selected">#{{ tag.label }}</QChip>
                        </QItemLabel>
                    </QItemSection>
                </QItem>
                <QItem>
                    <QItemSection>
                        <QItemLabel class="text-subtitle2">Icons</QItemLabel>
                        <QItemLabel lines="3">
                            <QChip
                                dense
                                :outline="!icon.selected"
                                :text-color="icon.selected ? 'white' : 'secondary'"
                                square
                                clickable
                                size="lg"
                                color="secondary"
                                v-for="(icon, index) in iconOptions"
                                :key="`icon-${index}`" ripple
                                v-model:selected="icon.selected"
                                @update:selected="clearOtherIcons(icon.value)">
                                <QIcon :name="icon.value"/>
                            </QChip>
                        </QItemLabel>
                    </QItemSection>
                </QItem>
                <QItem>
                    <QItemSection>
                        <QItemLabel class="text-subtitle2">People</QItemLabel>
                        <QItemLabel lines="5">
                            <UserChip
                                v-for="member in members"
                                :key="member.user.id"
                                :user="member.user"
                                :checked="false"
                                @user-selected="addUserFilter(member.user)"
                                @user-deselected="removeUserFilter(member.user)"/>
                        </QItemLabel>
                    </QItemSection>
                </QItem>
                <QItem>
                    <QItemSection>
                        <QItemLabel class="text-subtitle2">Sorting</QItemLabel>
                        <QItemLabel class="q-gutter-sm row" :lines="$q.screen.lt.md ? 2 : 1">
                            <QSelect
                                label="How do you want to sort?"
                                rounded
                                outlined
                                dense
                                class="col-lg-4 col-md-6"
                                v-model="sort"
                                :options="sortOptions" >
                                <template v-slot:selected-item="scope">
                                    {{ scope.opt.label }} ({{ scope.opt.ascending ? 'ascending' : 'descending' }})
                                </template>
                                <template v-slot:option="{ itemProps, selected, opt, toggleOption }">
                                    <QItem
                                        v-bind="itemProps"
                                        v-on="selected">
                                        <QItemSection>
                                            <QItemLabel v-text="opt.label"/>
                                        </QItemSection>
                                        <QItemSection side>
                                            <QToggle
                                                v-model="opt.ascending"
                                                @input="toggleOption(opt)"
                                                checked-icon="mdi-arrow-down-thick"
                                                unchecked-icon="mdi-arrow-up-thick"
                                                color="secondary"
                                                keep-color/>
                                        </QItemSection>
                                    </QItem>
                                </template>
                            </QSelect>
                        </QItemLabel>
                    </QItemSection>
                </QItem>
            </QList>
            <QPageSticky
                v-if="$q.screen.lt.md"
                position="right"
                :offset="[12,0]">
                <QBtn
                    icon="mdi-magnify"
                    fab-mini
                    class="full-width"
                    color="primary"
                    @click="hideMenu" />
            </QPageSticky>
            <QBtn
                icon="mdi-magnify"
                label="Search, Filter, and Sort"
                class="full-width"
                flat
                color="primary"
                @click="hideMenu" />
        </QMenu>
    </QSelect>
</template>

<script setup lang="ts">
import { computed, ref, reactive } from 'vue'

import { QMenu } from 'quasar'
import UserChip from '../components/UserChip.vue'

// Model
import { useStore } from '../stores'
import { GenericRef } from '../models/viewModels'

type TagFilter = {
    selected: boolean
    label: string
}

const $store = useStore()

const filterMenu = ref<QMenu>()
const searchInputs = ref();
const members = computed(() => $store.data.activeWorkspace?.members) // eslint-disable-line
const distinctTags = computed(() => {
    const tagSet = new Set<string>()

    if ($store.data.activeWorkspaceNotes) {
        // eslint-disable-next-line
        $store.data.activeWorkspaceNotes.forEach(m => {
            if (!m.tags || m.tags.length === 0) {
                return
            }

            m.tags.forEach(t => tagSet.add(t))
        })
    }

    const calculatedTags = new Array<TagFilter>()

    tagSet.forEach(t => {
        calculatedTags.push({ selected: false, label: t })
    })

    return calculatedTags.sort((a, b) => {
        if (a.label > b.label) return 1
        if (a.label < b.label) return -1
        return 0
    })
})

const tags = reactive(distinctTags.value)

const iconOptions = ref([
    { selected: false, value: 'mdi-alert-circle-outline', label: 'Alert - Circle' },
    { selected: false, value: 'mdi-lock-alert-outline', label: 'Alert - Lock' },
    { selected: false, value: 'mdi-bell-alert-outline', label: 'Alert - Bell' },
    { selected: false, value: 'mdi-account-alert-outline', label: 'Alert - User' },
    { selected: false, value: 'mdi-beaker-outline', label: 'Beaker / Sample' },
    { selected: false, value: 'mdi-calendar-month-outline', label: 'Calendar' },
    { selected: false, value: 'mdi-clipboard-check-outline', label: 'Clipboard / Checklist' },
    { selected: false, value: 'mdi-tooltip-image-outline', label: 'Diagram' },
    { selected: false, value: 'mdi-heart-outline', label: 'Heart' },
    { selected: false, value: 'mdi-pencil-circle-outline', label: 'Pencil' },
    { selected: false, value: 'mdi-ruler', label: 'Ruler' },
    { selected: false, value: 'mdi-star-outline', label: 'Star' },
    { selected: false, value: 'mdi-video-outline', label: 'Video' }
])

const sort = ref()

const sortOptions = reactive([
    { selected: false, label: 'Color', ascending: false },
    { selected: false, label: 'Created', ascending: false },
    { selected: false, label: 'Title', ascending: false }
]) // TODO: On server?

const messageToSearch = reactive({ title: '', icon: '', color: '', tags: [] })
const filters = computed(() => $store.data.filters)

function hideMenu() {
    // Hide the menu
    if (filterMenu.value) {
        filterMenu.value.hide()
    }
}

function applyFilters() {
    // Add filter for color
    if (messageToSearch.color !== '') {
        const index = filters.value.findIndex(f => f.type === 'color')

        if (index > -1) {
            $store.data.removeFilter(index);
        }

        $store.data.addFilter({
            label: messageToSearch.color || '',
            value: messageToSearch.color || '',
            type: 'color'
        });
    }

    // Add filter for tags
    tags.forEach((tag) => {
        if (!tag.selected) {
            return
        }

        if (filters.value.findIndex(f => f.type === 'tag' && f.value === tag.label) !== -1) {
            return
        }

        // eslint-disable-next-line
        void $store.data.addFilter({
            label: tag.label,
            value: tag.label,
            type: 'tag'
        });
    })

    // Add the icon options
    iconOptions.value.forEach((icon) => {
        if (!icon.selected) {
            return
        }

        if (filters.value.findIndex(f => f.type === 'icon' && f.value === icon.value) !== -1) {
            return
        }

        const index = filters.value.findIndex(f => f.type === 'icon')

        if (index > -1) {
            // eslint-disable-next-line
            void $store.data.removeFilter(index);
        }

        // eslint-disable-next-line
        void $store.data.addFilter({
            label: icon.label,
            value: icon.value,
            type: 'icon'
        });
    });

    // Add the sort option
    if (sort.value) {
        const index = filters.value.findIndex(f => f.type === 'sort')

        if (index > -1) {
            $store.data.removeFilter(index);
        }

        $store.data.addFilter({
            label: sort.value.label,
            value: sort.value.ascending ? 'ascending' : 'descending',
            type: 'sort'
        });
    }

    // Execute the search by pushing the filters.
    $store.data.setFilters(filters.value);
}

function addUserFilter(user: GenericRef) {
    if (filters.value.findIndex(f => f.type === 'user' && f.value === user.id) !== -1) {
        return;
    }

    $store.data.addFilter({
        label: user.name,
        value: user.id,
        type: 'user'
    });
}

function removeUserFilter(user: GenericRef) {
    const index = filters.value.findIndex(f => f.type === 'user' && f.value === user.id)

    $store.data.removeFilter(index);
}

function handleRemoveOption(option: any) {
    if (option.opt) {
        switch (option.opt.type) {
            case 'tag':
                tags.forEach(t => { if (t.label === option.opt.value) t.selected = false })
                break
            case 'icon':
                iconOptions.value.forEach(i => { if (i.value === option.opt.value) i.selected = false })
                break
            case 'color':
                messageToSearch.color = ''
                break
            case 'sort':
                sort.value = null
                break
        }
    }

    $store.data.removeFilter(option.index);
}

function handleClearOptions() {
    tags.forEach(t => { t.selected = false })
    iconOptions.value.forEach(i => { i.selected = false })

    messageToSearch.color = ''

    $store.data.clearFilters();
}

/**
 * Clears the other options for the icons which are not selected
 */
function clearOtherIcons(value: string) {
    iconOptions.value.forEach(i => {
        if (i.value === value) {
            return
        }

        i.selected = false
    })
}
</script>

