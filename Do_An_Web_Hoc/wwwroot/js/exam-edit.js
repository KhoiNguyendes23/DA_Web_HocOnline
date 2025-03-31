let quizIndex = 0;

window.onload = () => {
    const modelData = JSON.parse(document.getElementById("quiz-data-json").textContent);
    if (Array.isArray(modelData)) {
        modelData.forEach((quiz, i) => {
            addQuiz(quiz, i);
        });
    }
    document.querySelectorAll('.nav-link').forEach(btn => {
        btn.addEventListener('shown.bs.tab', () => {
            document.querySelector(btn.dataset.bsTarget).scrollIntoView({ behavior: 'smooth' });
        });
    });
};

function validateQuizTotalMarks() {
    const quizMarks = Array.from(document.querySelectorAll("input[name$='TotalMarks']")).map(i => parseInt(i.value) || 0);
    const total = quizMarks.reduce((sum, val) => sum + val, 0);
    const examTotal = parseInt(document.querySelector("#TotalMarks")?.value || 0);
    if (examTotal > 0 && total !== examTotal) {
        alert(`Tổng điểm các Quiz (${total}) phải bằng tổng điểm bài kiểm tra (${examTotal})`);
        return false;
    }
    return true;
}

function addQuiz(quizData = null, index = quizIndex) {
    const quizHtml = `
        <div class="card mb-4 quiz-block" data-quiz-index="${index}">
            <div class="card-header bg-info text-white d-flex justify-content-between">
                <span>Quiz ${index + 1}</span>
                <button type="button" class="btn btn-sm btn-danger" onclick="removeQuiz(this)">Xóa</button>
            </div>
            <div class="card-body">
                <div class="mb-3">
                    <label class="form-label">Tên Quiz</label>
                    <input name="Quizzes[${index}].QuizName" class="form-control" value="${quizData?.quizName || ''}" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Mô tả</label>
                    <input name="Quizzes[${index}].Description" class="form-control" value="${quizData?.description || ''}" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Tổng điểm Quiz</label>
                    <input name="Quizzes[${index}].TotalMarks" class="form-control quiz-points" value="${quizData?.totalMarks || 0}" oninput="validateQuizTotalMarks()" />
                </div>
                <div class="question-list"></div>
                <button type="button" class="btn btn-outline-primary mt-2" onclick="addQuestion(this, ${index})">Thêm câu hỏi</button>
            </div>
        </div>`;

    document.getElementById("quizContainer").insertAdjacentHTML("beforeend", quizHtml);
    if (quizData?.questions?.length) {
        const quizBlock = document.querySelector(`[data-quiz-index='${index}']`);
        quizData.questions.forEach((q, j) => addQuestion(quizBlock.querySelector(".btn-outline-primary"), index, q, j));
    }
    quizIndex++;
    updateQuizLabels();
    validateQuizTotalMarks();
}

function removeQuiz(button) {
    button.closest(".quiz-block").remove();
    updateQuizLabels();
    validateQuizTotalMarks();
}

function updateQuizLabels() {
    document.querySelectorAll(".quiz-block").forEach((el, i) => {
        el.querySelector(".card-header span").textContent = `Quiz ${i + 1}`;
    });
}

function addQuestion(button, quizIdx, questionData = null, questionCount = null) {
    const questionList = button.closest(".card-body").querySelector(".question-list");
    if (questionCount === null) questionCount = questionList.querySelectorAll(".question-block").length;

    const html = `
        <div class="border p-3 rounded mb-3 question-block">
            <label class="form-label">Câu hỏi ${questionCount + 1}</label>
            <input name="Quizzes[${quizIdx}].Questions[${questionCount}].QuestionText" value="${questionData?.questionText || ''}" class="form-control mb-2" />

            <label class="form-label">Option A</label>
            <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionA" value="${questionData?.optionA || ''}" class="form-control mb-2" />

            <label class="form-label">Option B</label>
            <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionB" value="${questionData?.optionB || ''}" class="form-control mb-2" />

            <label class="form-label">Option C</label>
            <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionC" value="${questionData?.optionC || ''}" class="form-control mb-2" />

            <label class="form-label">Option D</label>
            <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionD" value="${questionData?.optionD || ''}" class="form-control mb-2" />

            <label class="form-label">Đáp án đúng</label>
            <select name="Quizzes[${quizIdx}].Questions[${questionCount}].CorrectAnswer" class="form-select">
                <option value="A" ${questionData?.correctAnswer === 'A' ? 'selected' : ''}>A</option>
                <option value="B" ${questionData?.correctAnswer === 'B' ? 'selected' : ''}>B</option>
                <option value="C" ${questionData?.correctAnswer === 'C' ? 'selected' : ''}>C</option>
                <option value="D" ${questionData?.correctAnswer === 'D' ? 'selected' : ''}>D</option>
            </select>

            <button type="button" class="btn btn-sm btn-danger mt-2" onclick="this.closest('.question-block').remove()">Xóa câu hỏi</button>
        </div>`;

    questionList.insertAdjacentHTML("beforeend", html);
}
