﻿
using System.ComponentModel.DataAnnotations;

namespace QuizDev.Application.DTOs.Quizzes;

public class EditorQuizDto
{
    [Required(ErrorMessage = "Informe o título")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Informe a descrição")]
    public string Description { get; set; }

    public bool Expires { get; set; }
    public int ExpiresInSeconds { get; set; }
}
