let quizCount = 0;

function addQuiz() {
    const quizIndex = quizCount++;
    const quizBox = document.createElement("div");
    quizBox.className = "quiz-box border p-3 rounded mb-4";
    quizBox.dataset.index = quizIndex;

    quizBox.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-2">
            <h5 class="text-primary mb-0">Quiz <span class="quiz-number">${quizIndex + 1}</span></h5>
            <button type="button" class="btn btn-sm btn-danger" onclick="removeQuiz(this)">Xóa</button>
        </div>
        <div class="mb-3">
            <label class="form-label">Tên Quiz</label>
            <input name="Quizzes[${quizIndex}].QuizName" class="form-control" required />
        </div>
        <div class="mb-3">
            <label class="form-label">Mô tả</label>
            <input name="Quizzes[${quizIndex}].Description" class="form-control" />
        </div>
        <div class="mb-3">
            <label class="form-label">Tổng điểm Quiz</label>
            <input name="Quizzes[${quizIndex}].TotalMarks" class="form-control quiz-points" type="number" value="0" required oninput="updateQuizValidation()" />
        </div>
        <div class="mb-3">
            <label class="form-label">Danh sách câu hỏi</label>
            <button type="button" class="btn btn-sm btn-success mb-2" onclick="addQuestion(${quizIndex})">+ Thêm câu hỏi</button>
            <div id="quiz-${quizIndex}-questions"></div>
        </div>
    `;

    document.getElementById("quizList").appendChild(quizBox);
    updateQuizValidation();
}

function removeQuiz(button) {
    const box = button.closest(".quiz-box");
    if (box) box.remove();
    updateQuizValidation();
}

function updateQuizValidation() {
    const totalExamMarks = parseInt(document.querySelector("[name='TotalMarks']")?.value || 0);
    const quizBoxes = document.querySelectorAll(".quiz-box");
    let total = 0;
    quizBoxes.forEach(box => {
        const pointInput = box.querySelector(".quiz-points");
        if (pointInput) total += parseInt(pointInput.value || 0);
    });

    if (totalExamMarks > 0 && total !== totalExamMarks) {
        alert(`Tổng điểm các Quiz (${total}) phải bằng tổng điểm bài kiểm tra (${totalExamMarks})`);
    }
}

function addQuestion(quizIndex) {
    const container = document.getElementById(`quiz-${quizIndex}-questions`);
    const questionIndex = container.querySelectorAll(".question-box").length;
    const box = document.createElement("div");
    box.className = "question-box border p-3 rounded mb-3";

    box.innerHTML = `
        <label class="form-label fw-bold">Câu hỏi ${questionIndex + 1}</label>
        <input name="Quizzes[${quizIndex}].Questions[${questionIndex}].QuestionText" class="form-control mb-2" placeholder="Nhập nội dung câu hỏi" required>

        <label>Loại câu hỏi</label>
        <select name="Quizzes[${quizIndex}].Questions[${questionIndex}].QuestionType" class="form-select mb-2" onchange="toggleAnswerFields(this, ${quizIndex}, ${questionIndex})" required>
            <option value="">-- Chọn loại câu hỏi --</option>
            <option value="Tự luận">Tự luận</option>
            <option value="Trắc nghiệm">Trắc nghiệm</option>
        </select>

        <div class="answer-area" id="answer-area-${quizIndex}-${questionIndex}"></div>

        <button type="button" class="btn btn-danger mt-2" onclick="this.closest('.question-box').remove()">Xóa câu hỏi</button>
    `;

    container.appendChild(box);
}

function toggleAnswerFields(select, quizIndex, questionIndex) {
    const area = document.getElementById(`answer-area-${quizIndex}-${questionIndex}`);
    const type = select.value;

    if (type === "Trắc nghiệm") {
        area.innerHTML = `
            <label>Đáp án A</label>
            <input name="Quizzes[${quizIndex}].Questions[${questionIndex}].OptionA" class="form-control mb-2" required>

            <label>Đáp án B</label>
            <input name="Quizzes[${quizIndex}].Questions[${questionIndex}].OptionB" class="form-control mb-2" required>

            <label>Đáp án C</label>
            <input name="Quizzes[${quizIndex}].Questions[${questionIndex}].OptionC" class="form-control mb-2" required>

            <label>Đáp án D</label>
            <input name="Quizzes[${quizIndex}].Questions[${questionIndex}].OptionD" class="form-control mb-2" required>

            <label>Đáp án đúng</label>
            <select name="Quizzes[${quizIndex}].Questions[${questionIndex}].CorrectAnswer" class="form-select mb-2" required>
                <option value="">-- Chọn đáp án đúng --</option>
                <option value="A">A</option>
                <option value="B">B</option>
                <option value="C">C</option>
                <option value="D">D</option>
            </select>
        `;
    } else {
        area.innerHTML = ''; // Tự luận không cần đáp án
    }
}
