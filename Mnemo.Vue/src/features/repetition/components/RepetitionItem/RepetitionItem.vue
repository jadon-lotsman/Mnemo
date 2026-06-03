<script lang="ts" setup>
import { ref } from 'vue'
import type { RepetitionTask } from '../../types/RepetitionTask'
import RepetitionRadioItem from './RepetitionRadioItem.vue'
import RepetitionOrderItem from './RepetitionOrderItem.vue'

const props = defineProps<{
  listNumber: number
  task: RepetitionTask
}>()

const emits = defineEmits<{
  (e: 'submitAnswer', id: number, answer: string): void
}>()

const userAnswer = ref<string>('')
const placeholder = ref<string>('Type the translation...')

function submitAnswer() {
  emits('submitAnswer', props.task.id, userAnswer.value)
  placeholder.value = userAnswer.value
  userAnswer.value = ''
}
</script>

<template>
  <article class="task">
    <form @submit.prevent="submitAnswer">
      <header>
        <div class="prompt">
          <div>{{ listNumber + 1 }}.</div>

          <div v-if="task.taskType === 'yesorno'">
            <span>Does</span>
            <span class="bold">"{{ task.prompt }}"</span>
            <span>mean</span>
            <span class="bold">"{{ task.option }}"?</span>
          </div>
          <div v-else-if="task.taskType === 'parts'">
            <span>Put the parts of the sentence in order.</span>
          </div>
          <div v-else>
            <span>Translate</span>
            <span class="bold">"{{ task.prompt }}".</span>
          </div>
        </div>
        <!-- <div class="part-of-speech">(сущ.)</div> -->
      </header>
      <footer>
        <input
          v-if="task.taskType === 'text'"
          class="text-input"
          type="text"
          v-model="userAnswer"
          :placeholder="placeholder"
        />
        <div v-else-if="task.taskType === 'option'" class="option-input">
          <RepetitionRadioItem
            v-for="option in task.options"
            :key="option"
            :value="option"
            v-model="userAnswer"
          />
        </div>
        <div v-else-if="task.taskType === 'parts'">
          <RepetitionOrderItem v-model="userAnswer" :parts="task.sentenceParts" />
        </div>
        <div v-else-if="task.taskType === 'yesorno'" class="option-input">
          <RepetitionRadioItem :value="'yes'" v-model="userAnswer" />
          <RepetitionRadioItem :value="'no'" v-model="userAnswer" />
        </div>
      </footer>
      <button type="submit" class="big-button" :disabled="userAnswer === ''">Submit</button>
    </form>
  </article>
</template>

<style lang="scss" scoped>
.task {
  background-color: $plane-white;
  box-shadow: 5px 5px 0px $shadow;
  border-radius: 12px;

  max-width: 470px;
  min-width: 370px;
  border-radius: 12px;
  margin-bottom: 20px;

  header {
    display: flex;
    justify-content: space-between;
    flex-direction: row;
    padding: 12px 15px;

    .prompt {
      display: flex;
      justify-content: start;

      color: $gray-font;

      font-size: 16px;

      span {
        display: inline-block;
        margin-left: 5px;
      }

      .bold {
        color: $black-font;
      }
    }
  }

  footer {
    padding: 0px 15px;
    margin-bottom: 18px;

    .text-input {
      background-color: $plane-gray;
      color: $black-font;

      border-radius: 8px;
      padding: 7px 10px;

      width: 100%;

      font-size: 15px;
    }

    .option-input {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }
  }

  .big-button {
    background-color: $plane-gray;
  }
}
</style>
