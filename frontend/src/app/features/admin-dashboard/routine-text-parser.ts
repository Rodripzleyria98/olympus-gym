export interface ParsedRoutineExercise {
  nombre: string;
  seriesYRepeticiones: string;
  notas: string;
}

export interface ParsedRoutineDay {
  nombreDia: string;
  enfoque: string;
  ejercicios: ParsedRoutineExercise[];
}

const weekDays = 'lunes|martes|miércoles|miercoles|jueves|viernes|sábado|sabado|domingo';
const dayHeader = new RegExp(
  `^(?:#+\\s*)?(?:d[ií]a\\s*\\d+|${weekDays})(?:\\s*[:—–-]\\s*(.*))?$`,
  'i',
);
const setRepPattern = /\b\d+\s*[x×]\s*\d+(?:\s*[-–]\s*\d+)?(?:\s*(?:reps?|repeticiones))?\b/i;

export function parseRoutineText(text: string): ParsedRoutineDay[] {
  const days: ParsedRoutineDay[] = [];
  let currentDay: ParsedRoutineDay | null = null;

  for (const rawLine of text.split(/\r?\n/)) {
    const line = rawLine.trim()
      .replace(/^#{1,6}\s*/, '')
      .replace(/^\*\*(.*)\*\*$/, '$1')
      .replace(/^__(.*)__$/, '$1')
      .trim();
    if (!line) continue;

    const header = line.match(dayHeader);
    if (header) {
      const dayName = line.match(/^(?:d[ií]a\s*\d+|lunes|martes|miércoles|miercoles|jueves|viernes|sábado|sabado|domingo)/i)?.[0]
        ?? `Día ${days.length + 1}`;
      const focus = (header[1] ?? '').trim();
      currentDay = { nombreDia: dayName, enfoque: focus || 'Rutina importada', ejercicios: [] };
      days.push(currentDay);
      continue;
    }

    currentDay ??= { nombreDia: `Día ${days.length + 1}`, enfoque: 'Rutina importada', ejercicios: [] };
    if (!days.includes(currentDay)) days.push(currentDay);

    const exerciseLine = line.replace(/^(?:[-*•]\s*|\d+[.)]\s*)/, '').trim();
    const prescription = setRepPattern.exec(exerciseLine);
    if (!prescription || prescription.index === undefined) {
      currentDay.ejercicios.push({
        nombre: exerciseLine,
        seriesYRepeticiones: '',
        notas: '',
      });
      continue;
    }

    const exerciseName = exerciseLine.slice(0, prescription.index).replace(/[\s—–|:-]+$/, '').trim();
    const remaining = exerciseLine.slice(prescription.index + prescription[0].length);
    const notes = remaining.replace(/^[\s—–|:,-]+/, '').trim();
    currentDay.ejercicios.push({
      nombre: exerciseName || exerciseLine,
      seriesYRepeticiones: prescription[0].trim(),
      notas: notes,
    });
  }

  return days.filter(day => day.ejercicios.length > 0);
}
