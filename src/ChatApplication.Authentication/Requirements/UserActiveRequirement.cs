﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ChatApplication.Authentication.Requirements;

public class UserActiveRequirement : IAuthorizationRequirement
{
}