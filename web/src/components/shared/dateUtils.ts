import dayjs from 'dayjs'

export function useDateUtils() {
    function formatDateTime(value: string) {
        const age = Date.parse(value)

        return dayjs(age).fromNow()
    }

    return  {
        formatDateTime
    }
}